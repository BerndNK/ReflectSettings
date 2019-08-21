using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    [TestFixture]
    internal class EditableCollectionTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithListProperty
        {
            [UsedImplicitly]
            public List<string> StringList { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithObservableCollectionProperty
        {
            [UsedImplicitly]
            public ObservableCollection<string> StringList { get; set; }
        }
        
        [UsedImplicitly]
        private class ClassWithDictionary
        {
            public Dictionary<string, string> Dictionary { get; set; }
        }

        [Test]
        public void ListPropertyResultsInCollectionEditable()
        {
            var result = Produce<ClassWithListProperty>(out var instance);

            Assert.That(instance.StringList, Is.Not.Null);
        }


        [Test]
        public void ListEditableCanAddItems()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            var collectionProperty = (ICollection<string>) result.First(c => c.PropertyInfo.Name == nameof(ClassWithListProperty.StringList));
            
            collectionProperty.Add("");

            Assert.That(instance.StringList.Count, Is.EqualTo(1));
        }

        [Test]
        public void ListEditableCanModifyInstanceBeforeAddingIt()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            var editableCollection = result.OfType<IEditableCollection>().First();

            var subEditable = editableCollection.ItemToAddEditable as EditableString;
            const string valueThatGotAdded = nameof(valueThatGotAdded);
            subEditable.Value = valueThatGotAdded;
            editableCollection.AddNewItemCommand.Execute(null);

            Assert.That(instance.StringList.First(), Is.EqualTo(valueThatGotAdded));
        }

        [Test]
        public void ObservableCollectionPropertyResultsInCollectionEditable()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            
            Assert.That(instance.StringList, Is.Not.Null);
        }

        [Test]
        public void ChangingStringThroughEditableIsPossible()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            var editableCollection = result.OfType<IEditableCollection>().First();
            editableCollection.AddNewItemCommand.Execute(null);

            var stringEditable = editableCollection.SubEditables.OfType<EditableString>().First();
            const string someValue = nameof(someValue);
            stringEditable.Value = someValue;

            Assert.That(instance.StringList.First(), Is.EqualTo(someValue));
        }

        [Test]
        public void ChangingKeyThroughEditableUpdatesDictionary()
        {
            var result = Produce<ClassWithDictionary>(out var instance);
            var editableCollection = result.OfType<IEditableCollection>().First();
            editableCollection.ItemToAddEditable.Value = new KeyValuePair<string, string>("Key", "Value");
            editableCollection.AddNewItemCommand.Execute(null);

            var keyValuePair = editableCollection.SubEditables.OfType<IEditableKeyValuePair>().First();
            const string newKey = nameof(newKey);
            keyValuePair.Key = newKey;

            Assert.That(instance.Dictionary.First().Key, Is.EqualTo(newKey));
        }

        [Test]
        public void RemoveCommandRemovesItem()
        {
            var result = Produce<ClassWithDictionary>(out var instance);
            var editableCollection = result.OfType<IEditableCollection>().First();
            editableCollection.ItemToAddEditable.Value = new KeyValuePair<string, string>("Key", "Value");
            editableCollection.AddNewItemCommand.Execute(null);

            var itemToRemove = instance.Dictionary.First();

            editableCollection.RemoveItemCommand.Execute(itemToRemove);

            Assert.That(instance.Dictionary.Any(), Is.False);
        }

        [Test]
        public void SubEditablesGetAdditionalDataFromParent()
        {
            var result = Produce<ClassWithDictionary>(out var instance);
            var additionalData = new { };
            var editableCollection = result.OfType<IEditableCollection>().First();
            editableCollection.AdditionalData = additionalData;
            editableCollection.ItemToAddEditable.Value = new KeyValuePair<string, string>("Key", "Value");

            editableCollection.AddNewItemCommand.Execute(null);

            Assert.That(editableCollection.SubEditables.All(x => x.AdditionalData.Equals(additionalData)), Is.True);
        }
    }
}
