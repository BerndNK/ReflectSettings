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
    }
}