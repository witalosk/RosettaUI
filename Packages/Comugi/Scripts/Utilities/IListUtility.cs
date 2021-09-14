using System;
using System.Collections;
using UnityEngine;


namespace RosettaUI
{
    public static class IListUtility
    {
        public static IList AddItemAtLast(IList list, Type type, Type itemType)
        {
            if (list == null)
            {
                list = (IList)Activator.CreateInstance(type, 0);
            }

            var baseItem = list.Count > 0 ? list[list.Count - 1] : null;

            return AddItem(list, itemType, baseItem, list.Count);
        }

        public static IList RemoveItemAtLast(IList target, Type itemType)
        {
            return RemoveItem(target, itemType, target.Count - 1);
        }

        public static IList AddItem(IList list, Type elemType, object baseItem, int index)
        {
            index = Mathf.Clamp(index, 0, list.Count);
            var newElem = CreateNewItem(baseItem, elemType);

            if (list is Array array)
            {
                var newArray = Array.CreateInstance(elemType, array.Length + 1);
                Array.Copy(array, newArray, index);
                newArray.SetValue(newElem, index);
                Array.Copy(array, index, newArray, index + 1, array.Length - index);
                list = newArray;
            }
            else
            {
                list.Insert(index, newElem);
            }

            return list;
        }

        public static IList RemoveItem(IList list, Type itemType, int index)
        {
            if (list is Array array)
            {
                var newArray = Array.CreateInstance(itemType, array.Length - 1);
                Array.Copy(array, newArray, index);
                Array.Copy(array, index + 1, newArray, index, array.Length - 1 - index);
                list = newArray;
            }
            else
            {
                list.RemoveAt(index);
            }

            return list;
        }



        static object CreateNewItem(object baseItem, Type itemType)
        {
            object ret = null;

            if (baseItem != null)
            {
                // is cloneable
                var cloneable = baseItem as ICloneable;
                if (cloneable != null)
                {
                    ret = cloneable.Clone();
                }
                else if (itemType.IsValueType)
                {
                    ret = baseItem;
                }
                // has copy constructor
                else if (itemType.GetConstructor(new[] { itemType }) != null)
                {
                    ret = Activator.CreateInstance(itemType, baseItem);
                }
            }

            if (ret == null)
            {
                ret = (itemType == typeof(string))
                    ? ""
                    : Activator.CreateInstance(itemType);
            }

            return ret;
        }
    }
}