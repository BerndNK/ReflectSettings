using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    class NameAttributeTests : EditableConfigFactoryTestBase
    {
        private const string ShouldBeTheName = nameof(ShouldBeTheName);

        [UsedImplicitly]
        private class ClassWithDisplayNameProperty
        {
            [Name(ShouldBeTheName)]
            [UsedImplicitly]
            public string SomeString { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithoutDisplayNameProperty
        {
            [UsedImplicitly]
            public string SomeString { get; set; }
        }

        [Test]
        public void ClassWithDisplayNamePropertyGivesCorrectDisplayName()
        {
            var editableComplex = Produce<ClassWithDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            var stringEditable = editableComplex.SubEditables.First();

            Assert.That(stringEditable.DisplayName, Is.EqualTo(ShouldBeTheName));
        }

        [Test]
        public void ClassWithoutDisplayNamePropertyGivesPropertyNameAsDisplayName()
        {
            var editableComplex = Produce<ClassWithoutDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            var stringEditable = editableComplex.SubEditables.First();

            Assert.That(stringEditable.DisplayName, Is.EqualTo(nameof(ClassWithoutDisplayNameProperty.SomeString)));
        }
    }
}