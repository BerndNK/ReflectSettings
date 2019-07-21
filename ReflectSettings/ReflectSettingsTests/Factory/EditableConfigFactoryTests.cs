using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReflectSettings.Factory;

namespace ReflectSettingsTests.Factory
{
    [TestFixture]
    class EditableConfigFactoryTests
    {

        private class ClassWithDiverseProperties
        {
            public int IntProperty { get; set; }

            public string StringProperty { get; set; }

            public List<string> ListProperty { get; set; }

            public SubClass SubClass { get; set; }

            public List<SubClass> SubClasses { get; set; }
        }

        private class SubClass
        {
            public string SomeString { get; set; }

            public int SomeInt { get; set; }
        }

        [Test]
        public void IntPropertyShouldResultInConfigurableForInt()
        {
            var instance = new ClassWithDiverseProperties();
            var factory = new EditableConfigFactory();

            var result = factory.Produce(instance);
            var intProperty = result.First(c => c.PropertyInfo.PropertyType == typeof(int));

            Assert.That(intProperty.PropertyInfo.Name, Is.EqualTo(nameof(ClassWithDiverseProperties.IntProperty)));
        }
    }
}