using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    class NonEditableCollectionTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithReadOnlyListProperty
        {
            [TypesForInstantiation(typeof(List<string>))]
            public IReadOnlyList<string> ReadOnlyList { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithReadOnlyDictionaryProperty
        {
            [TypesForInstantiation(typeof(Dictionary<string,string>))]
            public IReadOnlyDictionary<string, string> ReadOnlyDictionary { get; set; }
        }

        [Test]
        public void ReadOnlyListPropertyResultsInNonEditableCollection()
        {
            var result = Produce<ClassWithReadOnlyListProperty>();

            var nonEditable = result.OfType<IReadOnlyEditableCollection>().FirstOrDefault();

            Assert.That(nonEditable, Is.Not.Null);
        }
        [Test]
        public void ReadOnlyDictionaryPropertyResultsInNonEditableCollection()
        {
            var result = Produce<ClassWithReadOnlyDictionaryProperty>();

            var nonEditable = result.OfType<IReadOnlyEditableCollection>().FirstOrDefault();

            Assert.That(nonEditable, Is.Not.Null);
        }
    }
}
