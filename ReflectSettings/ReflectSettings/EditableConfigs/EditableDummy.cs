﻿using System.Reflection;

namespace ReflectSettings.EditableConfigs
{
    public class EditableDummy : EditableConfigBase<object>
    {
        public EditableDummy(object forInstance, PropertyInfo propertyInfo, SettingsFactory factory) : base(forInstance, propertyInfo, factory)
        {
        }

        protected override object ParseValue(object value) => value;
    }
}