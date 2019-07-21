using System;
using System.Reflection;

namespace ReflectSettings.Factory.EditableConfigs
{
    public class EditableComplex : EditableConfigBase<object>
    {
        protected override object ParseValue(object value)
        {
            throw new NotImplementedException();
        }

        public EditableComplex(object forInstance, PropertyInfo propertyInfo) : base(forInstance, propertyInfo)
        {
        }
    }
}