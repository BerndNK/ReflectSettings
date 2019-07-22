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

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var config in e.NewItems.OfType<IEditableConfig>())
                    {
                        config.PropertyChanged += OnConfigValueChanged;
                    }

                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var config in e.OldItems.OfType<IEditableConfig>())
                    {
                        config.PropertyChanged -= OnConfigValueChanged;
                    }

                    break;
            }
        }

        private bool _suppressEvents = false;

        private void OnConfigValueChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_suppressEvents)
                return;
            _suppressEvents = true;

            foreach (var config in this.ToList())
            {
                config.UpdateCalculatedValues();
            }

            _suppressEvents = false;
        }
    }
}