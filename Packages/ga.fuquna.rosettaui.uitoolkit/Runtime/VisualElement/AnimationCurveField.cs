using System;
using RosettaUI.Builder;
using UnityEngine;
using UnityEngine.UIElements;

namespace RosettaUI.UIToolkit
{
    public class AnimationCurveField : BaseField<AnimationCurve>
    {
        public new static readonly string ussClassName = "rosettaui-gradient-field";
        public new static readonly string labelUssClassName = ussClassName + "__label";
        public new static readonly string inputUssClassName = ussClassName + "__input";

        public event Action<Vector2, AnimationCurveField> showAnimationCurveEditorFunc;

        private bool _valueNull;
        private readonly Background _mDefaultBackground = new();
        private readonly AnimationCurveInput _curveInput;
        
        public override AnimationCurve value
        {
            get => _valueNull ? null : AnimationCurveHelper.Clone(rawValue);
            set
            {
                if (value != null || !_valueNull)
                {
                    using var evt = ChangeEvent<AnimationCurve>.GetPooled(rawValue, value);
                    evt.target = this;
                    SetValueWithoutNotify(value);
                    SendEvent(evt);
                }
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnimationCurveField() : this(null)
        {
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public AnimationCurveField(string label) : base(label, new AnimationCurveInput())
        {
            _curveInput = this.Q<AnimationCurveInput>();
            AddToClassList(ussClassName);
            labelElement.AddToClassList(labelUssClassName);
        
            _curveInput.AddToClassList(inputUssClassName);
            _curveInput.RegisterCallback<ClickEvent>(OnClickInput);
            RegisterCallback<NavigationSubmitEvent>(OnNavigationSubmit);
        }
        
        private void ShowAnimationCurveEditor(Vector2 position)
        {
            showAnimationCurveEditorFunc?.Invoke(position, this);
        }
        
        private void UpdateAnimationCurveTexture()
        {
            var preview = _curveInput.preview;
            
            if (_valueNull || showMixedValue)
            {
                preview.style.backgroundImage = _mDefaultBackground;
            }
            else
            {
                AnimationCurveHelper.UpdateAnimationCurvePreviewToBackgroundImage(value, preview);
            }
        }
        
        private void OnClickInput(ClickEvent evt)
        {
            ShowAnimationCurveEditor(evt.position);
        
            evt.StopPropagation();
        }
        
        private void OnNavigationSubmit(NavigationSubmitEvent evt)
        {
            var mousePosition = Input.mousePosition;
            var position = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );
            
            var screenRect = new Rect(0f, 0f, Screen.width, Screen.height);
            if (!screenRect.Contains(position))
            {
                position = worldBound.center;
            }
            
            ShowAnimationCurveEditor(position);
        
            evt.StopPropagation();
        }
        
        public override void SetValueWithoutNotify(AnimationCurve newValue)
        {
            base.SetValueWithoutNotify(newValue);
        
            _valueNull = newValue == null;
            if (newValue != null)
            {
                value.keys = newValue.keys;
                value.postWrapMode = newValue.postWrapMode;
                value.preWrapMode = newValue.preWrapMode;
            }
            else // restore the internal gradient to the default state.
            {
                value.keys = new Keyframe[] {new Keyframe(0f, 0f), new Keyframe(1f, 1f)};
                value.postWrapMode = WrapMode.Clamp;
                value.preWrapMode = WrapMode.Clamp;
            }
        
            UpdateAnimationCurveTexture();
        }
        
        protected override void UpdateMixedValueContent()
        {
            if (showMixedValue)
            {
                _curveInput.preview.style.backgroundImage = _mDefaultBackground;
                _curveInput.preview.Add(mixedValueLabel);
            }
            else
            {
                UpdateAnimationCurveTexture();
                mixedValueLabel.RemoveFromHierarchy();
            }
        }
        
        public class AnimationCurveInput : VisualElement
        {
        
            public readonly VisualElement preview;
            
            public AnimationCurveInput()
            {
                preview = new VisualElement()
                {
                    style =
                    {
                        width = Length.Percent(100),
                        height = Length.Percent(100),
                    }
                };
                
                Add(preview);
            }
        }
        
    }
}