using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;

namespace ReflectSettingsTests.Attributes
{
    [TestFixture]
    internal class IgnoredForConfigAttributeTests : EditableConfigFactoryTestBase
    {
        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithIgnoredProperty
        {
            [UsedImplicitly]
            public int NotIgnored { get; set; }

            [IgnoredForConfig]
            [UsedImplicitly]
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
