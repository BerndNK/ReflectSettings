using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    internal class CalculatedVisibilityAttributeTests: EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithCalculatedVisibility
        {
            [CalculatedVisibility(nameof(IsSomeStringHidden))]
            public string SomeString { get; set; }

            public bool IsSomeStringVisible { get; set; }

            public bool IsSomeStringHidden() => !IsSomeStringVisible;
        }

        [Test]
        public void PropertyWithCalculatedVisibilityIsHiddenWhenMethodReturnsTrue()
        {
            var editables = Produce<ClassWithCalculatedVisibility>().ToList();
            var stringEditable = editables.OfType<EditableString>().First();
            var visibilitySetter = editables.OfType<EditableBool>().First();

            visibilitySetter.Value = true;

            Assert.That(stringEditable.IsHidden, Is.False);
        }

        [Test]
        public void PropertyWithCalculatedVisibilityIsHiddenWhenMethodReturnsFalse()
        {
            var editables = Produce<ClassWithCalculatedVisibility>().ToList();
            var stringEditable = editables.OfType<EditableString>().First();
            var visibilitySetter = editables.OfType<EditableBool>().First();

            visibilitySetter.Value = false;

            Assert.That(stringEditable.IsHidden, Is.True);
        }
    }
}
