﻿using System.Linq;
using NUnit.Framework;
using ReflectSettings.Factory.Attributes;

namespace ReflectSettingsTests.Factory.Attributes
{
    class PredefinedValuesAttributeTests : EditableConfigFactoryTestBase
    {
        private const string SomePredefinedValue = nameof(SomePredefinedValue);

        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithPredefinedValues
        {
            [PredefinedValues(5, 10)] 
            public int IntRestrictions { get; set; }

            [PredefinedValues(SomePredefinedValue)]
            public string StringRestrictions { get; set; }

            [PredefinedValues(SomeEnum.A, SomeEnum.B)]
            public SomeEnum EnumRestrictions { get; set; }

            internal enum SomeEnum
            {
                A,
                B,
                C
            }
        }

        [Test]
        public void IntPropertyWithPredefinedValuesDoesAllowPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.IntRestrictions)));
            const int someValue = 10;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public void StringPropertyWithPredefinedValuesDoesAllowPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.StringRestrictions)));
            const string someValue = SomePredefinedValue;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public void EnumPropertyWithPredefinedValuesDoesAllowPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.EnumRestrictions)));
            const ClassWithPredefinedValues.SomeEnum someValue = ClassWithPredefinedValues.SomeEnum.A;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }


        [Test]
        public void IntPropertyWithPredefinedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.IntRestrictions)));
            const int someValue = 3;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void StringPropertyWithPredefinedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.StringRestrictions)));
            const string someValue = "AnythingElse";
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void EnumPropertyWithPredefinedValuesDoesNotAllowAnythingElse()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.EnumRestrictions)));
            const ClassWithPredefinedValues.SomeEnum someValue = ClassWithPredefinedValues.SomeEnum.C;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }


        [Test]
        public void IntPropertyWithPredefinedValuesIsInitializedWithAPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest =
                result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.IntRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(5));
        }

        [Test]
        public void StringPropertyWithPredefinedValuesIsInitializedWithAPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.StringRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(SomePredefinedValue));
        }

        [Test]
        public void EnumPropertyWithPredefinedValuesIsInitializedWithAPredefinedValue()
        {
            var result = Produce<ClassWithPredefinedValues>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithPredefinedValues.EnumRestrictions)));

            Assert.That(toTest.Value, Is.EqualTo(ClassWithPredefinedValues.SomeEnum.A));
        }
    }
}