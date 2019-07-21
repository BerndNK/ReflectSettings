﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ReflectSettings.Attributes;

namespace ReflectSettings.EditableConfigs
{
    public abstract class EditableConfigBase<T> : IEditableConfig
    {
        private readonly IList<Attribute> _attributes;

        protected EditableConfigFactory Factory { get; }

        public object ForInstance { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public object Value
        {
            get => GetValue();
            set => SetValue(ParseValue(value));
        }

        protected abstract T ParseValue(object value);

        private T GetValue() => (T) PropertyInfo.GetValue(ForInstance);

        private void SetValue(T value) => PropertyInfo.SetValue(ForInstance, value);

        protected EditableConfigBase(object forInstance, PropertyInfo propertyInfo, EditableConfigFactory factory)
        {
            ForInstance = forInstance;
            PropertyInfo = propertyInfo;
            Factory = factory;

            _attributes = propertyInfo.GetCustomAttributes(true).OfType<Attribute>().ToList();
        }

        private TAttribute Attribute<TAttribute>() where TAttribute : Attribute =>
            _attributes.OfType<TAttribute>().FirstOrDefault() ?? Activator.CreateInstance<TAttribute>();

        protected MinMaxAttribute MinMax() => Attribute<MinMaxAttribute>();

        protected IEnumerable<T> PredefinedValues()
        {
            var staticValues = Attribute<PredefinedValuesAttribute>();
            var calculatedValuesAttribute = Attribute<CalculatedValuesAttribute>();

            var calculatedValues = calculatedValuesAttribute.CallMethod(ForInstance);

            var concat = staticValues.Values.Concat(calculatedValues).ToList();
            var toReturn = concat.OfType<T>().Except(ForbiddenValues()).ToList();

            if (concat.Any(x => x == null))
            {
                toReturn.Add(default);
            }

            return toReturn;
        }

        protected IEnumerable<T> ForbiddenValues()
        {
            var forbiddenValues = Attribute<ForbiddenValuesAttribute>().ForbiddenValues;

            return forbiddenValues.OfType<T>();
        }

        protected bool IsValueAllowed(T value)
        {
            if (value == null)
                return false;

            var predefinedValues = PredefinedValues().ToList();
            var isValueAllowed = predefinedValues.Count == 0 || predefinedValues.Any(v => v.Equals(value));
            var isValueForbidden = ForbiddenValues().Any(v => v.Equals(value));

            return isValueAllowed && !isValueForbidden;
        }

        protected bool IsNumericValueAllowed(dynamic numericValue)
        {
            var minMax = MinMax();

            if (!(numericValue is T asT))
                return false;

            if (!IsValueAllowed(asT))
                return false;

            return numericValue >= minMax.Min && numericValue <= minMax.Max;
        }

        protected bool TryCastNumeric(object value, out T result)
        {
            try
            {
                var castMethod = CastMethod();
                if (castMethod != null)
                    result = (T) castMethod(value);
                else
                    result = (T) value;
                return true;
            }
            catch (Exception)
            {
                result = default;
                return false;
            }
        }

        private Func<object, object> CastMethod()
        {
            var type = typeof(T);

            if (type == typeof(double))
                return x => Convert.ToDouble(x);

            if (type == typeof(int))
                return x => Convert.ToInt32(x);

            if (type == typeof(float))
                return x => (float) Convert.ToDouble(x);

            return x => x;
        }
    }
}