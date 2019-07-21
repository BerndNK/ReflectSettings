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
            var instance = Activator.CreateInstance(typeof(T));
            var factory = new EditableConfigFactory();

            return factory.Produce(instance).ToList();
        }
    }
}
