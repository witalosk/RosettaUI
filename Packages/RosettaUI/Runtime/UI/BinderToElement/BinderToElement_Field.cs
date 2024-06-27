﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace RosettaUI
{
    public static partial class BinderToElement
    {
        public static Element CreateFieldElement(LabelElement label, IBinder binder, in FieldOption option)
        {
            var valueType = binder.ValueType;

            if (BinderHistory.IsCircularReference(binder))
            {
                return CreateCircularReferenceElement(label, valueType);
            }

            using var binderHistory = BinderHistory.GetScope(binder);
            var optionCaptured = option;
            
            if (!UICustomCreationScope.IsIn(valueType) && UICustom.GetElementCreationFunc(valueType) is { } creationFunc)
            {
                using var scope = new UICustomCreationScope(valueType);
                return UI.NullGuardIfNeed(label, binder, () => creationFunc(label, binder));
            }

            return binder switch
            {
                IBinder<int> ib => new IntFieldElement(label, ib, option),
                IBinder<uint> ib => new UIntFieldElement(label, ib, option),
                IBinder<float> ib => new FloatFieldElement(label, ib, option),
                IBinder<string> ib => new TextFieldElement(label, ib, option),
                IBinder<bool> ib => new ToggleElement(label, ib),
                IBinder<Color> ib => new ColorFieldElement(label, ib),
                IBinder<Gradient> ib => UI.NullGuard(label, ib, () => new GradientFieldElement(label, ib)),
                _ when valueType.IsEnum => CreateEnumElement(label, binder),
                _ when TypeUtility.IsNullable(valueType) => CreateNullableFieldElement(label, binder, option),
                _ when typeof(IElementCreator).IsAssignableFrom(valueType) => CreateElementCreatorElement(label, binder),
                _ when ListBinder.IsListBinder(binder) && ListUtility.GetItemType(valueType).IsAbstract => CreateListView(label, binder),
                _ when ListBinder.IsListBinder(binder) => CreateListView(label, binder),
                _ when valueType.IsAbstract => CreateAbstractFieldElement(label, binder, false, option),

                _ => UI.NullGuardIfNeed(label, binder, () => CreateMemberFieldElement(label, binder, optionCaptured))
            };
        }

        private static Element CreateEnumElement(LabelElement label, IBinder binder)
        {
            var valueType = binder.ValueType;
            var enumToIdxBinder = EnumToIdxBinder.Create(binder);

            return new DropdownElement(label, enumToIdxBinder, Enum.GetNames(valueType));
        }
        
        private static Element CreateNullableFieldElement(LabelElement label, IBinder binder, FieldOption option)
        {
            var valueBinder = NullableToValueBinder.Create(binder);
            return UI.NullGuard(label, binder, () => CreateFieldElement(label, valueBinder, option));
        }

        private static Element CreateElementCreatorElement(LabelElement label, IBinder binder)
        {
            var lastObject = binder.GetObject();
            if (binder.ValueType.IsValueType)
            {
                var elementCreator = lastObject as IElementCreator;
                Assert.IsNotNull(elementCreator);
                var element = elementCreator.CreateElement(label);
                SetCallBinderSetterWhenValueChanged(element);
                return element;
            }
            
            // ElementCreatorの場合、参照が変わったらUIを作り直す
            return UI.NullGuard(label, binder, () =>
                UI.DynamicElementOnTrigger
                (
                    rebuildIf: _ =>
                    {
                        var current = binder.GetObject();
                        var refChanged = !ReferenceEquals(lastObject, binder.GetObject());
                        lastObject = current;
                        return refChanged;
                    },
                    () =>
                    {
                        if (lastObject is IElementCreator elementCreator)
                        {
                            label?.DetachView();
                            var element = elementCreator.CreateElement(label);
                            SetCallBinderSetterWhenValueChanged(element);
                            return element;
                        }

                        Debug.LogWarning($"{binder.ValueType} is not {nameof(IElementCreator)}");
                        return null;
                    }
                )
            );

            void SetCallBinderSetterWhenValueChanged(Element element)
            {
                element?.RegisterValueChangeCallback(() => binder.SetObject(lastObject));
            }
        }

        private static Element CreateListView(LabelElement label, IBinder binder)
        {
            var option = (binder is IPropertyOrFieldBinder pfBinder)
                ? new ListViewOption(
                    reorderable: TypeUtility.IsReorderable(pfBinder.ParentBinder.ValueType, pfBinder.PropertyOrFieldName)
                )
                : ListViewOption.Default;


            return UI.List(label, binder, null, option);
        }

        private static Element CreateMemberFieldElement(LabelElement label, IBinder binder, in FieldOption option)
        {
            var valueType = binder.ValueType;
            var optionCaptured = option;

            // UICustomCreationScopeをキャンセル
            // クラスのメンバーに同じクラスがある場合はUICustomを有効にする
            using var uiCustomScope = new UICustomCreationScope(null);

            var elements = TypeUtility.GetUITargetFieldNames(valueType).Select(fieldName =>
            {
                var fieldBinder = PropertyOrFieldBinder.Create(binder, fieldName);
                var fieldLabel = UICustom.ModifyPropertyOrFieldLabel(valueType, fieldName);

                Element targetElement;

                // 属性によるElementの変更
                var rangeAttr = TypeUtility.GetRange(valueType, fieldName);
                if (rangeAttr is not null)
                {
                    var (minGetter, maxGetter) = RangeUtility.CreateGetterMinMax(rangeAttr, fieldBinder.ValueType);
                    targetElement = UI.Slider(fieldLabel, fieldBinder, minGetter, maxGetter);
                }
                else
                {
                    targetElement = UI.Field(fieldLabel, fieldBinder, optionCaptured);
                }
                
                // 属性によるElementの付加, Propertyの変更
                List<Element> innerElements = new();
                foreach (var attr in TypeUtility.GetPropertyAttributes(valueType, fieldName))
                {
                    switch (attr)
                    {
                        case HeaderAttribute headerAttr:
                            innerElements.Add(UI.Space().SetHeight(18f));
                            innerElements.Add(UI.Label($"<b>{headerAttr.header}</b>"));
                            break;
                        case SpaceAttribute spaceAttr:
                            innerElements.Add(UI.Space().SetHeight(spaceAttr.height));
                            break;
                        case MultilineAttribute _ when targetElement is TextFieldElement textField:
                            textField.IsMultiLine = true;
                            break;
                    }
                }
                
                if (innerElements.Count == 0)
                {
                    return targetElement;
                }
                
                innerElements.Add(targetElement);

                return UI.Column(innerElements);
            });


            Element ret;
            if (TypeUtility.IsSingleLine(valueType))
                ret = new CompositeFieldElement(label, elements);
            else if (label != null)
                ret = UI.Fold(label, elements);
            else
                ret = UI.Column(elements);

            return ret;
        }
        
        private static Element CreateCircularReferenceElement(LabelElement label, Type type)
        {
            return new CompositeFieldElement(label,
                new[]
                {
                    new HelpBoxElement($"[{type}] Circular reference detected.", HelpBoxType.Error)
                }).SetInteractable(false);
        }
    }
}
