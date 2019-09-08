﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs.InheritingAttribute;
using ReflectSettings.Utils;

namespace ReflectSettings.EditableConfigs
{
    public interface IEditableConfig : INotifyPropertyChanged, IRemoveTracking
    {
        PropertyInfo PropertyInfo { get; set; }

        object Value { get; set; }

        ObservableCollection<object> PredefinedValues { get; }

        bool HasPredefinedValues { get; }

        ChangeTrackingManager ChangeTrackingManager { get; set; }

        InheritedAttributes<CalculatedTypeAttribute> CalculatedTypes { get; }

        InheritedAttributes<CalculatedValuesAttribute> CalculatedValues { get; }

        InheritedAttributes<CalculatedValuesAsyncAttribute> CalculatedValuesAsync { get; }

        bool IsDisplayNameProperty { get; }

        void UpdateCalculatedValues();

        event EventHandler<EditableConfigValueChangedEventArgs> ValueChanged;

        string DisplayName { get; }

        /// <summary>
        /// Property to store custom data into
        /// </summary>
        object AdditionalData { get; set; }

        bool HasCalculatedType { get; }

        bool IsHidden { get; }

        bool IsBusy { get; }
    }
}