using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    /// Itemを追加するとき最後のアイテムをコピーするListViewController
    /// Unity2022以降はUnityのInternalにアクセスしなくてOK
    /// </summary>
    ///
#if UNITY_2022_1_OR_NEWER
    public class ListViewControllerCustom : ListViewController
#else
    internal class ListViewControllerCustom : ListViewController
#endif
    {
        public Func<object> createNewInstance { get; set; } = null;
        
#if !UNITY_2022_1_OR_NEWER
        public virtual int GetItemsCount()
        {
            var source = this.itemsSource;
            return source?.Count ?? 0;
        }
#endif
        
        public override void AddItems(int itemCount)
        {
            if (itemCount <= 0) return;

            var previousCount = GetItemsCount();
            var intList = ListPool<int>.Get();
            try
            {
                var type = itemsSource.GetType();
                var itemType = ListUtility.GetItemType(type);
                for (var i = 0; i < itemCount; ++i)
                {
                    itemsSource = createNewInstance == null 
                        ? ListUtility.AddCopiedItemAtLast(itemsSource, type, itemType)
                        : ListUtility.AddItemAtLast(itemsSource, type, itemType, createNewInstance.Invoke());
                    
                    intList.Add(previousCount + i);
                }
                
                RaiseItemsAdded(intList);
            }
            finally
            {
                CollectionPool<List<int>, int>.Release(intList);
            }
            
            RaiseOnSizeChanged();
        }
    }
}