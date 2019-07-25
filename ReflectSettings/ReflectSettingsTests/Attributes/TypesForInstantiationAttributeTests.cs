using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    internal class TypesForInstantiationAttributeTests : EditableConfigFactoryTestBase
    {
        private interface ISomeObject
        {
        }

        private class SomeObject : ISomeObject
        {
        }

        [UsedImplicitly]
        private class ClassWithInterfaceProperty
        {
            [TypesForInstantiation(typeof(SomeObject))]
            public ISomeObject SomeObject { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithInterfaceInList
        {
            [TypesForInstantiation(typeof(SomeObject))]
            public List<ISomeObject> SomeObjects { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithIList
        {
            [TypesForInstantiation(typeof(List<ISomeObject>), typeof(SomeObject))]
            public ICollection<ISomeObject> SomeObjects { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithIListAndObservableCollection
        {
            [TypesForInstantiation(typeof(ObservableCollection<ISomeObject>), typeof(SomeObject))]
            public ICollection<ISomeObject> SomeObjects { get; set; }
        }

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
        public void GivenCorrectTypesForInstantiationShouldResultInInstance()
        {
            Produce<ClassWithInterfaceProperty>(out var instance);

            Assert.That(instance.SomeObject.GetType(), Is.EqualTo(typeof(SomeObject)));
        }

        [Test]
        public void GivenCorrectTypesForInstantiationOfListElementsShouldResultInInstance()
        {
            var result = Produce<ClassWithInterfaceInList>(out var instance);

            var listEditable = result.OfType<IEditableCollection>().First();
            listEditable.AddNewItemCommand.Execute(null);

            Assert.That(instance.SomeObjects.First().GetType(), Is.EqualTo(typeof(SomeObject)));
        }

        [Test]
        public void GivenListTypeForInstantiationOfCollectionShouldResultInList()
        {
            var result = Produce<ClassWithIList>(out var instance);

            Assert.That(instance.SomeObjects.GetType(), Is.EqualTo(typeof(List<ISomeObject>)));
        }

        [Test]
        public void GivenTwoTypesForListAndSubTypeShouldResultInListAndSubType()
        {
            var result = Produce<ClassWithIList>(out var instance);
            var listEditable = result.OfType<IEditableCollection>().First();
            listEditable.AddNewItemCommand.Execute(null);

            Assert.That(instance.SomeObjects.First().GetType(), Is.EqualTo(typeof(SomeObject)));
        }

        [Test]
        public void GivenObservableCollectionTypeForInstantiationOfCollectionShouldResultInList()
        {
            var result = Produce<ClassWithIListAndObservableCollection>(out var instance);

            Assert.That(instance.SomeObjects.GetType(), Is.EqualTo(typeof(ObservableCollection<ISomeObject>)));
        }

        [Test]
        public void ReadOnlyListPropertyIsInitializedWithList()
        {
            Produce<ClassWithReadOnlyListProperty>(out var instance);

            Assert.That(instance.ReadOnlyList, Is.Not.Null);
        }

        [Test]
        public void ReadOnlyDictionaryPropertyIsInitializedWithDictionary()
        {
            Produce<ClassWithReadOnlyDictionaryProperty>(out var instance);

            Assert.That(instance.ReadOnlyDictionary, Is.Not.Null);
        }
    }
}