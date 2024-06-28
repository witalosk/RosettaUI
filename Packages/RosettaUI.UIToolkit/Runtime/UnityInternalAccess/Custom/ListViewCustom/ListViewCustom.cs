using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit.UnityInternalAccess
{
    /// <summary>
    /// ListView with Additional function
    /// 
    ///  Add item
    ///  - avoid error when IList item is ValueType
    ///  - duplicate a previous item or execute callback when + button clicked
    /// 
    ///  Support for external List changes
    /// </summary>
    public class ListViewCustom : ListView
    {
        public Func<object> createNewInstance { get; set; }
        
        #region Avoid error when IList item is ValueType
        
#if UNITY_2022_2_OR_NEWER
        protected override CollectionViewController CreateViewController()
        {
            var controller = new ListViewControllerCustom();
            controller.createNewInstance = createNewInstance;
            return controller;
        }
#else
        private protected override void CreateViewController()
        {
            var controller = new ListViewControllerCustom();
            controller.createNewInstance = createNewInstance;
            SetViewController(controller);
        }
#endif

        #endregion
        

        #region DynamicHeightVirtualizationController
        
        private static FieldInfo _fieldInfo;
        private protected override void CreateVirtualizationController()
        {
            if (virtualizationMethod != CollectionVirtualizationMethod.DynamicHeight)
            {
                base.CreateVirtualizationController();
                return;
            }

            _fieldInfo ??= typeof(BaseVerticalCollectionView).GetField("m_VirtualizationController", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(_fieldInfo);
            _fieldInfo.SetValue(this, new DynamicHeightVirtualizationControllerCustom(this));
            // _fieldInfo.SetValue(this, new DynamicHeightVirtualizationControllerForDebug<ReusableListViewItem>(this));
        }

        #endregion
    }
}