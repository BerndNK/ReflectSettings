using System;
using System.Collections.Generic;
using System.Linq;
using ReflectSettings.Factory;
using ReflectSettings.Factory.EditableConfigs;

namespace ReflectSettingsTests.Factory
{
    internal abstract class EditableConfigFactoryTestBase
    {
        protected IEnumerable<IEditableConfig> Produce<T>()
        {
            return Produce<T>(out var unused);
        }

        protected IEnumerable<IEditableConfig> Produce<T>(out T instance)
        {
            instance = (T) Activator.CreateInstance(typeof(T));
            var factory = new EditableConfigFactory();

            return factory.Produce(instance).ToList();
        }
    }
}