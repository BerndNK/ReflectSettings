using System.Linq;
using NUnit.Framework;
using ReflectSettings.Factory.Attributes;

namespace ReflectSettingsTests.Factory.Attributes
{
    [TestFixture]
    class IgnoredForConfigAttributeTests : EditableConfigFactoryTestBase
    {
        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithIgnoredProperty
        {
            public int NotIgnored { get; set; }

            [IgnoredForConfig]
            public int Ignored { get; set; }
        }

        [Test]
        public void IgnoredPropertyShouldBeIgnored()
        {
            var result = Produce<ClassWithIgnoredProperty>();

            Assert.False(result.Any(c => c.PropertyInfo.Name.Equals(nameof(ClassWithIgnoredProperty.Ignored))));
        }

        [Test]
        public void NotIgnoredPropertyShouldNotBeIgnored()
        {
            var result = Produce<ClassWithIgnoredProperty>();

            Assert.True(result.Any(c => c.PropertyInfo.Name.Equals(nameof(ClassWithIgnoredProperty.NotIgnored))));
        }
    }
}
