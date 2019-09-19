using System;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests
{
    [TestFixture]
    class ChangeTrackingManagerTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithComplexProperty
        {
            public SubClass SubClass { get; set; }
        }
        
        [UsedImplicitly]
        private class SubClass
        {
            public string SomeString { get; set; }
        }

        [Test]
        public void InstanceWithComplexPropertyShouldHaveNotNullProperty()
        {
            var instance = (ClassWithComplexProperty) Activator.CreateInstance(typeof(ClassWithComplexProperty));
            var factory = new SettingsFactory();
            var changeTrackingManager = new ChangeTrackingManager();
            var result = factory.Reflect(instance, changeTrackingManager).ToList();
            var configChangedCalled = 0;
            changeTrackingManager.ConfigurationChanged += (sender, args) => configChangedCalled += 1;

            var complexProperty = (EditableComplex<SubClass>) result.First(c => c.PropertyInfo.Name == nameof(ClassWithComplexProperty.SubClass));
            var subClassEditable = complexProperty.SubEditables.First();

            subClassEditable.Value = "anything";

            Assert.That(configChangedCalled, Is.EqualTo(1));
        }
    }
}
