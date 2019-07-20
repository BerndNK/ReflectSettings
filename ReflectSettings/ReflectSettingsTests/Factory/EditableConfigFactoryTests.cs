using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ReflectSettings.Factory;
using ReflectSettings.Factory.Attributes;

namespace ReflectSettingsTests.Factory
{
    [TestFixture]
    class EditableConfigFactoryTests
    {
        private class ClassWithIgnoredProperty
        {
            public int NotIgnored { get; set; }

            [IgnoredForConfig]
            public int Ignored { get; set; }
        }

        private class ClassWithDiverseProperties
        {
            [MinMax(20, 25)]
            public int IntProperty { get; set; }

            [PredefinedValues("OptionA", "OptionB", "OptionC")]
            public string StringProperty { get; set; }

            public List<string> ListProperty { get; set; }
        }

        [Test]
        public void IgnoredPropertyShouldNotBeIgnored()
        {
            var instance = new ClassWithIgnoredProperty();
            var factory = new EditableConfigFactory();

            var result = factory.Produce(instance);

            Assert.False(result.Any(c => c.PropertyInfo.Name.Equals(nameof(ClassWithIgnoredProperty.Ignored))));
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