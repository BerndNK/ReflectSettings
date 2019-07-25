using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    internal class EditableKeyValuePairTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithDictionary
        {
            public Dictionary<string, int> Dictionary { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithKeyValuePairProperty
        {
            public KeyValuePair<string, int> KeyValuePair { get; set; }
        }

        [Test]
        public void ClassWithDictionaryShouldResultInCollectionOfEditableKeyValuePair()
        {
            var result = Produce<ClassWithDictionary>(out var instance);

            var editable = result.OfType<IEditableCollection>().FirstOrDefault()?.ItemToAddEditable as IEditableKeyValuePair;

            Assert.That(editable, Is.Not.Null);
        }

        [Test]
        public void ClassWithDictionaryShouldBeInitializedWithNewDictionary()
        {
            Produce<ClassWithDictionary>(out var instance);

            Assert.That(instance.Dictionary, Is.Not.Null);
        }

        [Test]
        public void ClassWithKeyValuePairPropertyCanUpdateItsValue()
        {
            var result = Produce<ClassWithKeyValuePairProperty>(out var instance);

            var editableKeyValuePair = result.OfType<IEditableKeyValuePair>().First();
            var keyValuePair = new KeyValuePair<string, int>("key", 0);
            editableKeyValuePair.Value = keyValuePair;

            Assert.That(instance.KeyValuePair, Is.EqualTo(keyValuePair));
        }

        [Test]
        public void EditableKeyValuePairCanChangeKey()
        {
            var result = Produce<ClassWithKeyValuePairProperty>(out var instance);

            var editableKeyValuePair = result.OfType<IEditableKeyValuePair>().First();
            const string setValue = "Key";
            editableKeyValuePair.Key = setValue;

            Assert.That(instance.KeyValuePair.Key, Is.EqualTo(setValue));
        }

        [Test]
        public void EditableKeyValuePairCanChangeValue()
        {
            var result = Produce<ClassWithKeyValuePairProperty>(out var instance);

            var editableKeyValuePair = result.OfType<IEditableKeyValuePair>().First();
            const int setValue = 8;
            editableKeyValuePair.PairValue = setValue;

            Assert.That(instance.KeyValuePair.Value, Is.EqualTo(setValue));
        }
    }
}