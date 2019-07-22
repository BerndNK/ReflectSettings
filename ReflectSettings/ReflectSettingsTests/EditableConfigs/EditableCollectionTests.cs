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

        [Test]
        public void ListPropertyResultsInCollectionEditable()
        {
            var result = Produce<ClassWithListProperty>(out var instance);

            Assert.That(instance.StringList, Is.Not.Null);
        }


        [Test]
        public void ListEdtiableCanAddItems()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            var collectionProperty = (ICollection<string>) result.First(c => c.PropertyInfo.Name == nameof(ClassWithListProperty.StringList));
            
            collectionProperty.Add("");

            Assert.That(instance.StringList.Count, Is.EqualTo(1));
        }

        [Test]
        public void ObservableCollectionPropertyResultsInCollectionEditable()
        {
            var result = Produce<ClassWithListProperty>(out var instance);
            
            Assert.That(instance.StringList, Is.Not.Null);
        }
    }
}
