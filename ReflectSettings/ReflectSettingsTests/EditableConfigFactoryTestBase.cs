using System;
using System.Collections.Generic;
using System.Linq;
using ReflectSettings;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests
{
    internal abstract class EditableConfigFactoryTestBase
    {
        protected IEnumerable<IEditableConfig> Produce<T>(bool useConfigurableItself = false)
        {
            return Produce<T>(out var unused, useConfigurableItself);
        }

        protected IEnumerable<IEditableConfig> Produce<T>(out T instance, bool useConfigurableItself = false)
        {
            instance = (T) Activator.CreateInstance(typeof(T));
            var factory = new SettingsFactory();

            return factory.Reflect(instance, out var changeTrackingManager, useConfigurableItself).ToList();
        }
    }
}