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
            var result = Produce<ClassWithListProperty>();
            var collectionProperty = result.First(c => c.PropertyInfo.Name == nameof(ClassWithListProperty.StringList)) as EditableCollection<List<string>, string>;
            
            Assert.That(collectionProperty, Is.Not.Null);
        }

        [Test]
        public void ObservableCollectionPropertyResultsInCollectionEditable()
        {
            var result = Produce<ClassWithListProperty>();
            var collectionProperty = result.First(c => c.PropertyInfo.Name == nameof(ClassWithObservableCollectionProperty.StringList)) as EditableCollection<ObservableCollection<string>, string>;
            
            Assert.That(collectionProperty, Is.Not.Null);
        }
    }
}
