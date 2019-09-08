using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;

namespace ReflectSettingsTests.Attributes
{
    class IsHiddenAttributeTests : EditableConfigFactoryTestBase
    {

        [UsedImplicitly]
        private class ClassWithIsHiddenProperty
        {
            [IsHidden]
            public string SomeString { get; set; }

        }

        [UsedImplicitly]
        private class ClassWithoutIsHiddenProperty
        {
            public string SomeString { get; set; }
        }
        
        [Test]
        public void ClassWithoutIsHiddenPropertyIsHidden()
        {
            var editable = Produce<ClassWithIsHiddenProperty>().First();

            Assert.That(editable.IsHidden, Is.True);
        }

        [Test]
        public void ClassWithIsHiddenPropertyIsNotHidden()
        {
            var editable = Produce<ClassWithoutIsHiddenProperty>().First();

            Assert.That(editable.IsHidden, Is.False);
        }
    }
}
