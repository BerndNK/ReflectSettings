using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    internal class CalculatedValuesAttributeTests : EditableConfigFactoryTestBase
    {
        private const string SomeRestrictedValue = nameof(SomeRestrictedValue);

        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithCalculatedValues
        {
            [CalculatedValues(nameof(IntRestrictionsValues))]
            [UsedImplicitly]
            public int IntRestrictions { get; set; }
            
            [UsedImplicitly]
            public IEnumerable<int> IntRestrictionsValues()
            {
                yield return 5;
            }

            [CalculatedValues(nameof(StringRestrictionsValues))]
            [UsedImplicitly]
            public string StringRestrictions { get; set; }
            
            [UsedImplicitly]
            public IEnumerable<string> StringRestrictionsValues()
            {
                yield return SomeRestrictedValue;
            }

            [CalculatedValues(nameof(EnumRestrictionsValues))]
            [UsedImplicitly]
            public SomeEnum EnumRestrictions { get; set; }
            
            [UsedImplicitly]
            public IEnumerable<SomeEnum> EnumRestrictionsValues()
            {
                yield return SomeEnum.A;
                yield return SomeEnum.B;
            }

            internal enum SomeEnum
            {
                A,
                B,
                C
            }
        }

        [UsedImplicitly]
        private class ClassWithSubClassThatIsConfigurable
        {
            public const string SomePredefinedValue = nameof(SomePredefinedValue);
            public const string SomeOtherPredefinedValue = nameof(SomeOtherPredefinedValue);
            public const string SubClassValueCalculatorKey = nameof(SubClassValueCalculatorKey);

            [UsedImplicitly]
            public IEnumerable<string> PossibleValuesForSubType()
            {
                yield return SomePredefinedValue;
                yield return SomeOtherPredefinedValue;
            }

            [CalculatedValues(nameof(PossibleValuesForSubType), SubClassValueCalculatorKey)]
            public SubClassThatTakesValuesFromParent SubClass { get; set; }
        }
        
        [UsedImplicitly]
        private class SubClassThatTakesValuesFromParent
        {
            [CalculatedValues(ClassWithSubClassThatIsConfigurable.SubClassValueCalculatorKey)]
            public string StringWithParentValues { get; set; }
        }

        private class ClassWithConfigurableValues
        {
            [UsedImplicitly]
            [CalculatedValues(nameof(SelectedValueValues))]
            public string SelectedValue { get; set; }

            public int SomeInt { get; set; }
            
            [IgnoredForConfig]
            public List<string> DefinedValues { get; } = new List<string>();

            [UsedImplicitly]
            public IEnumerable<string> SelectedValueValues() => DefinedValues;
        }

        [UsedImplicitly]
        private class ClassWhichProvidesValuesForListOfSubClass
        {
            public const string AllowedSubClassValue1 = nameof(AllowedSubClassValue1);
            public const string AllowedSubClassValue2 = nameof(AllowedSubClassValue2);

            public IEnumerable<string> AllowedStringsForRestrictedList()
            {
                yield return AllowedSubClassValue1;
                yield return AllowedSubClassValue2;
            }

            [TypesForInstantiation(typeof(List<RestrictedStringList>))]
            [CalculatedValues(nameof(AllowedStringsForRestrictedList), nameof(AllowedStringsForRestrictedList))]
            public IList<RestrictedStringList> SubInstancesWithRestrictedList { get; set; }
        }

        [UsedImplicitly]
        private class RestrictedStringList
        {
            [TypesForInstantiation(typeof(List<string>))]
            [CalculatedValues(nameof(ClassWhichProvidesValuesForListOfSubClass.AllowedStringsForRestrictedList), true)]
            public IList<string> SelectedStrings { get; set; }
        }

        [UsedImplicitly]
        private class ClassWhichProvidesValuesForListOfSubClassWithinASecondSubclass
        {
            public IEnumerable<string> AllowedStringsForRestrictedList() => ValueProvidingSubClasses.Select(x => x.Name);

            [TypesForInstantiation(typeof(List<ClassWithName>))]
            public IList<ClassWithName> ValueProvidingSubClasses { get; set; }

            [TypesForInstantiation(typeof(List<RestrictedStringList>))]
            [CalculatedValues(nameof(AllowedStringsForRestrictedList), nameof(AllowedStringsForRestrictedList))]
            public IList<RestrictedStringList> SubInstancesWithRestrictedList { get; set; }
        }

        private class ClassWithName
        {
            public string Name { get; set; }
        }

        [Test]
        public void IntPropertyWithCalculatedValuesDoesAllowCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.IntRestrictions)));
            const int someValue = 5;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public void StringPropertyWithCalculatedValuesDoesAllowCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.StringRestrictions)));
            const string someValue = SomeRestrictedValue;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public void EnumPropertyWithCalculatedValuesDoesAllowCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.EnumRestrictions)));
            const ClassWithCalculatedValues.SomeEnum someValue = ClassWithCalculatedValues.SomeEnum.A;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }


        [Test]
        public void IntPropertyWithCalculatedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.IntRestrictions)));
            const int someValue = 3;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void StringPropertyWithCalculatedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.StringRestrictions)));
            const string someValue = "AnythingElse";
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void EnumPropertyWithCalculatedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.EnumRestrictions)));
            const ClassWithCalculatedValues.SomeEnum someValue = ClassWithCalculatedValues.SomeEnum.C;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }


        [Test]
        public void IntPropertyWithCalculatedValuesIsInitializedWithACalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.IntRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(5));
        }

        [Test]
        public void StringPropertyWithCalculatedValuesIsInitializedWithACalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.StringRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(SomeRestrictedValue));
        }

        [Test]
        public void EnumPropertyWithCalculatedValuesIsInitializedWithACalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValues.EnumRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(ClassWithCalculatedValues.SomeEnum.A));
        }

        [Test]
        public void UpdatingValuesDoesTriggerRecalculationOfPredefinedValues()
        {
            var result = Produce<ClassWithConfigurableValues>(out var instance);

            const string someString = nameof(someString);
            instance.DefinedValues.Add(someString);

            result.First(x => x.PropertyInfo.Name == nameof(ClassWithConfigurableValues.SomeInt)).Value = 5;

            Assert.That(instance.SelectedValue, Is.EqualTo(someString));
        }
        
        [Test]
        public void ValueCalculatorOnParentClassGetsUsedOnSubClassForDefaultValue()
        {
            Produce<ClassWithSubClassThatIsConfigurable>(out var instance);

            Assert.That(instance.SubClass?.StringWithParentValues, Is.EqualTo(ClassWithSubClassThatIsConfigurable.SomePredefinedValue));
        }
        
        [Test]
        public void ValueCalculatorOnParentClassGetsUsedOnSubClassForForbiddingValue()
        {
            var result = Produce<ClassWithSubClassThatIsConfigurable>(out var instance);

            var editableString = result.OfType<IEditableComplex>().First().SubEditables.OfType<EditableString>().First();

            editableString.Value = "anything";

            Assert.That(instance.SubClass?.StringWithParentValues, Is.EqualTo(ClassWithSubClassThatIsConfigurable.SomePredefinedValue));
        }
        
        [Test]
        public void ValueCalculatorOnParentClassGetsUsedOnSubClassForChangingValue()
        {
            var result = Produce<ClassWithSubClassThatIsConfigurable>(out var instance);
            
            var editableString = result.OfType<IEditableComplex>().First().SubEditables.OfType<EditableString>().First();

            editableString.Value = ClassWithSubClassThatIsConfigurable.SomeOtherPredefinedValue;

            Assert.That(instance.SubClass?.StringWithParentValues, Is.EqualTo(ClassWithSubClassThatIsConfigurable.SomeOtherPredefinedValue));
        }

        [Test]
        public void CalculatedAttributeForListResultsInListForbiddingValuesOtherThanCalculatedValues()
        {
            var result = Produce<ClassWhichProvidesValuesForListOfSubClass>(out var instance);

            var collectionEditable = result.OfType<IEditableCollection>().First();
            collectionEditable.AddNewItemCommand.Execute(null);

            var collectionEditableOfSubClass = collectionEditable.SubEditables.OfType<IEditableComplex>().First().SubEditables.OfType<IEditableCollection>().First();
            var restrictedEditable = (EditableString) collectionEditableOfSubClass.ItemToAddEditable;

            const string anyValue = nameof(anyValue);
            restrictedEditable.Value = anyValue;
            collectionEditableOfSubClass.AddNewItemCommand.Execute(null);

            Assert.That(instance.SubInstancesWithRestrictedList.First().SelectedStrings.First(), Is.Not.EqualTo(anyValue));
        }

        [Test]
        public void CalculatedAttributeForListResultsInListAddingCalculatedValues()
        {
            var result = Produce<ClassWhichProvidesValuesForListOfSubClass>(out var instance);

            var collectionEditable = result.OfType<IEditableCollection>().First();
            collectionEditable.AddNewItemCommand.Execute(null);

            var collectionEditableOfSubClass = collectionEditable.SubEditables.OfType<IEditableComplex>().First().SubEditables.OfType<IEditableCollection>().First();
            collectionEditableOfSubClass.AddNewItemCommand.Execute(null);

            Assert.That(instance.SubInstancesWithRestrictedList.First().SelectedStrings.First(), Is.EqualTo(ClassWhichProvidesValuesForListOfSubClass.AllowedSubClassValue1));
        }

        [Test]
        public void CalculatedAttributeForListResultsInListOnlyAllowingCalculatedValues()
        {
            var result = Produce<ClassWhichProvidesValuesForListOfSubClass>(out var instance);

            var collectionEditable = result.OfType<IEditableCollection>().First();
            collectionEditable.AddNewItemCommand.Execute(null);

            var collectionEditableOfSubClass = collectionEditable.SubEditables.OfType<IEditableComplex>().First().SubEditables.OfType<IEditableCollection>().First();
            var restrictedEditable = (EditableString) collectionEditableOfSubClass.ItemToAddEditable;

            restrictedEditable.Value = ClassWhichProvidesValuesForListOfSubClass.AllowedSubClassValue2;
            collectionEditableOfSubClass.AddNewItemCommand.Execute(null);

            Assert.That(instance.SubInstancesWithRestrictedList.First().SelectedStrings.First(), Is.EqualTo(ClassWhichProvidesValuesForListOfSubClass.AllowedSubClassValue2));
        }

        [Test]
        public void SubClassChangeShouldTriggerCalculatedValuesOfAnotherSubClass()
        {
            var result = Produce<ClassWhichProvidesValuesForListOfSubClassWithinASecondSubclass>(out var instance).ToList();
            var nameProviderCollection = (IEditableCollection) result.First(x =>
                x.PropertyInfo.Name == nameof(ClassWhichProvidesValuesForListOfSubClassWithinASecondSubclass
                    .ValueProvidingSubClasses));
            nameProviderCollection.AddNewItemCommand.Execute(null);

            var nameProvidingEditable = nameProviderCollection.SubEditables.OfType<IEditableComplex>().First().SubEditables.OfType<EditableString>().First();

            const string initialValue = nameof(initialValue);
            nameProvidingEditable.Value = initialValue;


            var subClassCollection = (IEditableCollection) result.First(x =>
                x.PropertyInfo.Name == nameof(ClassWhichProvidesValuesForListOfSubClassWithinASecondSubclass
                    .SubInstancesWithRestrictedList));
            subClassCollection.AddNewItemCommand.Execute(null);


            var stringListEditable = subClassCollection.SubEditables.OfType<IEditableComplex>().First().SubEditables.OfType<IEditableCollection>().First();
            stringListEditable.AddNewItemCommand.Execute(null);

            var restrictedStringEditable = stringListEditable.SubEditables.OfType<EditableString>().First();
            Assert.That(restrictedStringEditable.Value, Is.EqualTo(initialValue));

            nameProvidingEditable.Value = "somethingElse";

            Assert.That(restrictedStringEditable.Value, Is.Not.EqualTo(initialValue));
        }
    }
}