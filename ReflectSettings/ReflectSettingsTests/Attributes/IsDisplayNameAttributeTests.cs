using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    class IsDisplayNameAttributeTests : EditableConfigFactoryTestBase
    {
        private const string ShouldNotBeTheName = nameof(ShouldNotBeTheName);
        private const string ShouldBeTheName = nameof(ShouldBeTheName);

        [UsedImplicitly]
        private class ClassWithIsDisplayNameProperty
        {
            [IsDisplayName] 
            [UsedImplicitly]
            public string SomeString { get; set; } = ShouldBeTheName;

            public override string ToString() => ShouldNotBeTheName;
        }
        
        [UsedImplicitly]
        private class ClassWithoutIsDisplayNameProperty
        {
            public override string ToString() => ShouldBeTheName;
        }

        [UsedImplicitly]
        private class ClassWithChildrenWithIsDisplayNameProperty
        {
            [UsedImplicitly]
            public List<ClassWithIsDisplayNameProperty> Children { get; set; }
        }

        [UsedImplicitly]
        private class ClassWithChildrenWithoutIsDisplayNameProperty
        {
            [UsedImplicitly]
            public List<ClassWithoutIsDisplayNameProperty> Children { get; set; }
        }
        
        [Test]
        public void ClassWithIsDisplayNamePropertyGivesCorrectDisplayName()
        {
            var editableComplex = Produce<ClassWithIsDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            Assert.That(editableComplex.DisplayName, Is.EqualTo(ShouldBeTheName));
        }
        
        [Test]
        public void ClassWithIsDisplayNamePropertyShowsDisplayNameAfterChanging()
        {
            var editableComplex = Produce<ClassWithIsDisplayNameProperty>(true).OfType<IEditableComplex>().First();
            

            var namePropertyEditable = editableComplex.SubEditables.First();
            const string newName = nameof(newName);
            namePropertyEditable.Value = newName;

            Assert.That(editableComplex.DisplayName, Is.EqualTo(newName));
        }
        
        [Test]
        public void ClassWithIsDisplayNamePropertyThrowsCorrectPropertyChangedAfterChanging()
        {
            var editableComplex = Produce<ClassWithIsDisplayNameProperty>(true).OfType<IEditableComplex>().First();

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
        public void ClassWithoutIsDisplayNamePropertyGivesToStringAsDisplayName()
        {
            var editableComplex = Produce<ClassWithoutIsDisplayNameProperty>(true).OfType<IEditableComplex>().First();

            Assert.That(editableComplex.DisplayName, Is.EqualTo(ShouldBeTheName));
        }

        [Test]
        public void ChildrenOfClassWithChildrenWithIsDisplayNamePropertyShouldGivePropertyForDisplayName()
        {
            var editableCollection = Produce<ClassWithChildrenWithIsDisplayNameProperty>().OfType<IEditableCollection>().First();

            editableCollection.AddNewItemCommand.Execute(null);

            var editableOfFirstChild = editableCollection.SubEditables.OfType<IEditableComplex>().First();

            Assert.That(editableOfFirstChild.DisplayName, Is.EqualTo(ShouldBeTheName));
        }

        [Test]
        public void ChildrenOfClassWithChildrenWithoutIsDisplayNamePropertyShouldGiveToStringAsDisplayName()
        {
            var editableCollection = Produce<ClassWithChildrenWithoutIsDisplayNameProperty>().OfType<IEditableCollection>().First();

            editableCollection.AddNewItemCommand.Execute(null);

            var editableOfFirstChild = editableCollection.SubEditables.OfType<IEditableComplex>().First();

            Assert.That(editableOfFirstChild.DisplayName, Is.EqualTo(ShouldBeTheName));
        }
    }
}
