using System;
using System.Linq.Expressions;

namespace RosettaUI
{
    public static partial class UI
    {
        public static Element AbstractField<T>(Expression<Func<T>> targetExpression, in bool typeChangeable = false, FieldOption? option = null)
        {
            return AbstractField(ExpressionUtility.CreateLabelString(targetExpression), targetExpression, typeChangeable, option);
        }

        public static Element AbstractField<T>(LabelElement label, Expression<Func<T>> targetExpression, bool typeChangeable = false, in FieldOption? option = null)
        {
            var binder = UIInternalUtility.CreateBinder(targetExpression);
            return AbstractField(label, binder, typeChangeable, option);
        }

        public static Element AbstractField<T>(Expression<Func<T>> targetExpression, Action<T> writeValue, bool typeChangeable = false, in FieldOption? option = null)
            => AbstractField(ExpressionUtility.CreateLabelString(targetExpression), targetExpression.Compile(), writeValue, typeChangeable, option);
        
        public static Element AbstractField<T>(LabelElement label, Func<T> readValue, Action<T> writeValue, bool typeChangeable = false, in FieldOption? option = null)
            => AbstractField(label, Binder.Create(readValue, writeValue), typeChangeable, option);

        public static Element AbstractField(LabelElement label, IBinder binder, bool typeChangeable = false, in FieldOption? option = null)
        {
            var element = BinderToElement.CreateAbstractFieldElement(label, binder, typeChangeable, option ?? FieldOption.Default);
            if (element != null) UIInternalUtility.SetInteractableWithBinder(element, binder);

            return element;
        }

        public static Element AbstractFieldReadOnly<T>(Expression<Func<T>> targetExpression)
            => AbstractFieldReadOnly(
                ExpressionUtility.CreateLabelString(targetExpression),
                ExpressionUtility.CreateReadFunc(targetExpression
                ));

        public static Element AbstractFieldReadOnly<T>(LabelElement label, Func<T> readValue)
            => AbstractField(label, readValue, null);
    }
}