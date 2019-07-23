using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    internal class EditableBoolTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithBoolProperties
        {
            public bool AllowAnything { get; set; }
        }

        [Test]
        public void ClassWithBoolPropertyShouldResultInEditableBol()
        {
            var result = Produce<ClassWithBoolProperties>(out var instance);

            var boolEditable = result.OfType<EditableBool>().FirstOrDefault();

            Assert.That(boolEditable, Is.Not.Null);
        }
    }
}
