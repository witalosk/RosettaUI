﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.Builder
{
    public static class ElementBindingExtension
    {
        public static void SetValueWithoutNotifyIfNotEqual<T>(this INotifyValueChanged<T> field, T value)
        {
            if (!EqualityComparer<T>.Default.Equals(field.value, value))
            {
                field.SetValueWithoutNotify(value);
            }
        }

        public static void SubscribeValueOnUpdateCallOnce<T>(this in ReadOnlyValueElement<T>.ViewBridge viewBridge,  INotifyValueChanged<T> field)
        {
            viewBridge.SubscribeValueOnUpdateCallOnce(field.SetValueWithoutNotifyIfNotEqual);
        }

        public static void ListenLabel<T>(this BaseField<T> field, LabelElement labelElement)
        {
            labelElement.GetViewBridge().SubscribeValueOnUpdateCallOnce(str => field.label = str);
        }

        public static void Bind<T>(this  BaseField<T> field, FieldBaseElement<T> element)
        {
            element.GetViewBridge().SubscribeValueOnUpdateCallOnce(field);
            field.RegisterValueChangedCallback(evt => element.OnViewValueChanged(evt.newValue));

            // ラベルのChangeEventを潰しておく
            // fieldが BaseField<string> だとラベルのChangeEventを受け取ってしまうのでそれを止める
            field.labelElement.RegisterValueChangedCallback(evt => evt.StopPropagation());
        }

        public static void Bind<TFieldValue, TElementValue>(
            this INotifyValueChanged<TFieldValue> field,
            FieldBaseElement<TElementValue> element,
            Func<TElementValue, TFieldValue> elementValueToFieldValue,
            Func<TFieldValue, TElementValue> fieldValueToElementValue
        )
        {
            element.GetViewBridge().SubscribeValueOnUpdateCallOnce(v => field.SetValueWithoutNotifyIfNotEqual(elementValueToFieldValue(v)));
            field.RegisterValueChangedCallback(evt => element.OnViewValueChanged(fieldValueToElementValue(evt.newValue)));
        }
    }
}