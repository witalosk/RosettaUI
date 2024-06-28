using System;
using System.Linq;
using UnityEngine;

namespace RosettaUI
{
    public static partial class BinderToElement
    {
        public static Element CreateAbstractFieldElement(LabelElement label, IBinder binder, bool typeChangeable = false, FieldOption? option = null)
        {
            var valueType = binder.ValueType;
            
            if (ListBinder.IsListBinder(binder) && ListUtility.GetItemType(valueType).IsAbstract)
            {
                return CreateAbstractListView(label, binder, typeChangeable);
            }
            if (!valueType.IsAbstract)
            {
                return CreateFieldElement(label, binder, option ?? FieldOption.Default);
            }

            var candidateTypes = TypeUtility.GetConcreteTypesFromAbstractType(valueType).ToList();
            if (candidateTypes.Count < 1)
            {
                return new CompositeFieldElement(label, new[] {
                    new HelpBoxElement($"[{valueType}] No concrete type found.", HelpBoxType.Error)
                }).SetInteractable(false);
            }
            object obj = binder.GetObject();
            int selectedIdx = obj == null ? 0 : candidateTypes.IndexOf(obj.GetType()) + 1;
            typeChangeable = typeChangeable && obj is not MonoBehaviour;
            
            return UI.Fold(
                label, 
                UI.Dropdown("Type", () => selectedIdx, typeChangeable ? idx => 
                    {
                        obj = idx == 0 ? null : Activator.CreateInstance(candidateTypes[idx - 1]);
                        binder.SetObject(obj);
                        selectedIdx = idx;
                    } : null,
                    new []{ "<null>" }.Concat(candidateTypes.Select(t => t.Name))
                ),
                UI.DynamicElementOnStatusChanged(
                    () => selectedIdx, 
                    idx => idx == 0 ? null : UI.Field(null, Binder.Create(obj, obj?.GetType()), option ?? FieldOption.Default)
                )
            );
        }
        
        private static Element CreateAbstractListView(LabelElement label, IBinder binder, bool typeChangeable = false)
        {
            var option = (binder is IPropertyOrFieldBinder pfBinder)
                ? new ListViewOption(
                    reorderable: TypeUtility.IsReorderable(pfBinder.ParentBinder.ValueType, pfBinder.PropertyOrFieldName)
                )
                : ListViewOption.Default;

            return UI.List(label, binder, (b, i)=> UI.AbstractField($"Item {i}", b, typeChangeable), () => null, option);
        }
    }
}