using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using ReflectSettings.EditableConfigs;

namespace ReflectSettings
{
    public class ChangeTrackingManager : ObservableCollection<IEditableConfig>
    {
        public ChangeTrackingManager()
        {
            CollectionChanged += OnCollectionChanged;
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
                    }

                    foreach (var config in e.OldItems.OfType<IEditableCollection>())
                    {
                        config.CollectionChanged -= InvokeCollectionChanged;
                    }

                    break;
            }
        }

        private void InvokeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }

        private bool _suppressEvents = false;

        private void OnConfigValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IEditableConfig.Value))
                return;
            if (_suppressEvents)
                return;
            _suppressEvents = true;

            foreach (var config in this.ToList())
            {
                config.UpdateCalculatedValues();
            }

            _suppressEvents = false;
            ConfigurationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}