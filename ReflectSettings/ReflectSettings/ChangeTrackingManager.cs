using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ReflectSettings.EditableConfigs;

namespace ReflectSettings
{
    public class ChangeTrackingManager : ObservableCollection<IEditableConfig>
    {
        public ChangeTrackingManager()
        {
            CollectionChanged += OnCollectionChanged;
            _registeredInstancesWithEditables = new Dictionary<INotifyPropertyChanged, Dictionary<PropertyInfo, IEditableConfig>>();
            _asyncSynchronizer = new AsyncSynchronizer();
        }

        public event EventHandler ConfigurationChanged;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var config in e.NewItems.OfType<IEditableConfig>())
                    {
                        config.PropertyChanged += OnConfigValueChanged;
                    }

                    foreach (var config in e.NewItems.OfType<IEditableCollection>())
                    {
                        config.CollectionChanged += InvokeCollectionChanged;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var config in e.OldItems.OfType<IEditableConfig>())
                    {
                        config.PropertyChanged -= OnConfigValueChanged;
                        RemoveFromRegisteredInstances(config);
                    }

                    foreach (var config in e.OldItems.OfType<IEditableCollection>())
                    {
                        config.CollectionChanged -= InvokeCollectionChanged;
                    }

                    break;
            }
        }

        private void RemoveFromRegisteredInstances(IEditableConfig config)
        {
            var dictionaryEntriesWhichContainConfig =_registeredInstancesWithEditables.Where(x => x.Value.ContainsValue(config)).ToList();

            foreach (var keyValuePair in dictionaryEntriesWhichContainConfig)
            {
                var dictionary = keyValuePair.Value;
                var tuplesToRemove = dictionary.Where(x => x.Value == config).ToList();
                foreach (var tuple in tuplesToRemove)
                {
                    dictionary.Remove(tuple.Key);
                }

                if (dictionary.Count == 0)
                    _registeredInstancesWithEditables.Remove(keyValuePair.Key);
            }
        }

        private void InvokeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _suppressEvents;

        public IDisposable SuppressEvents(bool callConfigurationChangedWhenDisposed = false)
        {
            return new EventSuppressor(this, callConfigurationChangedWhenDisposed);
        }

        private class EventSuppressor : IDisposable
        {
            private readonly ChangeTrackingManager _manager;

            private readonly bool _callConfigurationChangedWhenDisposed;

            private readonly bool _didSuppress;

            public EventSuppressor(ChangeTrackingManager manager, bool callConfigurationChangedWhenDisposed)
            {
                _manager = manager;
                _callConfigurationChangedWhenDisposed = callConfigurationChangedWhenDisposed;
                if (!manager._suppressEvents)
                {
                    _didSuppress = true;
                    _manager._suppressEvents = true;
                }
            }

            public void Dispose()
            {
                if (_didSuppress)
                {
                    _manager._suppressEvents = false;
                    if (_callConfigurationChangedWhenDisposed)
                        _manager.OnConfigValueChanged(this, new PropertyChangedEventArgs(nameof(IEditableConfig.Value)));
                }
            }
        }

        private void OnConfigValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IEditableConfig.Value) && e.PropertyName != nameof(IEditableCollection.SelectedValue))
                return;
            if (_suppressEvents || _asyncSynchronizer.IsBusy)
                return;
            _suppressEvents = true;

            foreach (var config in this.ToList())
            {
                config.UpdateCalculatedValues();
            }

            _asyncSynchronizer.UpdateAll(this);

            _suppressEvents = false;
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private readonly Dictionary<INotifyPropertyChanged, Dictionary<PropertyInfo, IEditableConfig>> _registeredInstancesWithEditables;
        private AsyncSynchronizer _asyncSynchronizer;

        public void RegisterPropertyChangedListener(INotifyPropertyChanged configurable, IEditableConfig editableConfig)
        {
            CreateEntryForInstance(configurable);

            var propertyDictionary = _registeredInstancesWithEditables[configurable];
            AddToPropertyDictionary(propertyDictionary, editableConfig);
            SetUpEventListener(configurable, editableConfig);
        }

        private void SetUpEventListener(INotifyPropertyChanged configurable, IEditableConfig editableConfig)
        {
            configurable.PropertyChanged += (sender, args) =>
            {
                if (_suppressEvents || args.PropertyName != editableConfig.PropertyInfo.Name)
                    return;

                editableConfig.ValueWasExternallyChanged();
            };
        }

        private void AddToPropertyDictionary(Dictionary<PropertyInfo, IEditableConfig> propertyDictionary, IEditableConfig editableConfig)
        {
            if (propertyDictionary.ContainsKey(editableConfig.PropertyInfo))
                return;

            propertyDictionary.Add(editableConfig.PropertyInfo, editableConfig);
        }

        private void CreateEntryForInstance(INotifyPropertyChanged instance)
        {
            if (_registeredInstancesWithEditables.ContainsKey(instance))
                return;

            _registeredInstancesWithEditables.Add(instance, new Dictionary<PropertyInfo, IEditableConfig>());
        }
    }
}