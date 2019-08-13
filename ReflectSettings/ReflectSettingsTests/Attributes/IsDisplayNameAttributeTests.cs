using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    class DisplayNameAttributeTests : EditableConfigFactoryTestBase
    {
        private const string ShouldNotBeTheName = nameof(ShouldNotBeTheName);
        private const string ShouldBeTheName = nameof(ShouldBeTheName);

        [UsedImplicitly]
        private class ClassWithDisplayNameProperty
        {
            [IsDisplayName] 
            [UsedImplicitly]
            public string SomeString { get; set; } = ShouldBeTheName;

            public override string ToString() => ShouldNotBeTheName;
        }
        
        [UsedImplicitly]
        private class ClassWithoutDisplayNameProperty
        {
            public override string ToString() => ShouldBeTheName;
        }

        [UsedImplicitly]
        private class ClassWithChildrenWithDisplayNameProperty
        {
            [UsedImplicitly]
            public List<ClassWithDisplayNameProperty> Children { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithChildrenWithoutDisplayNameProperty
        {
            [UsedImplicitly]
            public List<ClassWithoutDisplayNameProperty> Children { get; set; }
        }
        
        [Test]
        public void ClassWithDisplayNamePropertyGivesCorrectDisplayName()
        {
            var editableComplex = Produce<ClassWithDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            Assert.That(editableComplex.DisplayName, Is.EqualTo(ShouldBeTheName));
        }
        
        [Test]
        public void ClassWithDisplayNamePropertyShowsDisplayNameAfterChanging()
        {
            var editableComplex = Produce<ClassWithDisplayNameProperty>(true).OfType<IEditableComplex>().First();
            

            var namePropertyEditable = editableComplex.SubEditables.First();
            const string newName = nameof(newName);
            namePropertyEditable.Value = newName;

            Assert.That(editableComplex.DisplayName, Is.EqualTo(newName));
        }
        
        [Test]
        public void ClassWithDisplayNamePropertyThrowsCorrectPropertyChangedAfterChanging()
        {
            var editableComplex = Produce<ClassWithDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            var propertyChangedCalled = false;
            editableComplex.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(IEditableComplex.DisplayName))
                    propertyChangedCalled = true;
            };

            var namePropertyEditable = editableComplex.SubEditables.First();
            namePropertyEditable.Value = "NewName";

            Assert.That(propertyChangedCalled, Is.True);
        }

        [Test]
        public void ClassWithoutDisplayNamePropertyGivesToStringAsDisplayName()
        {
            var editableComplex = Produce<ClassWithoutDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            Assert.That(editableComplex.DisplayName, Is.EqualTo(ShouldBeTheName));
        }

        [Test]
        public void ChildrenOfClassWithChildrenWithDisplayNamePropertyShouldGivePropertyForDisplayName()
        {
            var editableCollection = Produce<ClassWithChildrenWithDisplayNameProperty>().OfType<IEditableCollection>().First();

            editableCollection.AddNewItemCommand.Execute(null);

            var editableOfFirstChild = editableCollection.SubEditables.OfType<IEditableComplex>().First();

            Assert.That(editableOfFirstChild.DisplayName, Is.EqualTo(ShouldBeTheName));
        }

        [Test]
        public void ChildrenOfClassWithChildrenWithoutDisplayNamePropertyShouldGiveToStringAsDisplayName()
        {
            var editableCollection = Produce<ClassWithChildrenWithoutDisplayNameProperty>().OfType<IEditableCollection>().First();

            editableCollection.AddNewItemCommand.Execute(null);

            var editableOfFirstChild = editableCollection.SubEditables.OfType<IEditableComplex>().First();

            Assert.That(editableOfFirstChild.DisplayName, Is.EqualTo(ShouldBeTheName));
        }
    }
}
