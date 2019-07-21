using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Factory.Attributes;

namespace ReflectSettingsTests.Factory.Attributes
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
    }
}