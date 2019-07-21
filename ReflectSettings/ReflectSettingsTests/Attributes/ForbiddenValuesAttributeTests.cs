using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;

namespace ReflectSettingsTests.Attributes
{
    [TestFixture]
    internal class ForbiddenValuesAttributeTests : EditableConfigFactoryTestBase
    {
        private const string SomeRestrictedStringValue = nameof(SomeRestrictedStringValue);

        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithForbiddenValues
        {
            [UsedImplicitly]
            public string AnythingGoes { get; set; }

            [ForbiddenValues(0,5,10)]
            [UsedImplicitly]
            public int IntRestrictions { get; set; }

            [ForbiddenValues(SomeRestrictedStringValue)]
            [UsedImplicitly]
            public string StringRestrictions { get; set; }

            [ForbiddenValues(SomeEnum.A, SomeEnum.B)]
            [UsedImplicitly]
            public SomeEnum EnumRestrictions { get; set; }

            
            [ForbiddenValues(null)]
            [PredefinedValues(SomeRestrictedStringValue)]
            [UsedImplicitly]
            public string NonNullString { get; set; }

            internal enum SomeEnum
            {
                A,
                B,
                [UsedImplicitly]
                C
            }
        }

        [Test]
        public void PropertyWithoutForbiddenValuesAllowsAnything()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.AnythingGoes)));
            const string someValue = "AppleBanana";
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }
        
        [Test]
        public void IntPropertyWithForbiddenValuesDoesNotAllowForbidden()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.IntRestrictions)));
            const int someValue = 0;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }
        
        [Test]
        public void StringPropertyWithForbiddenValuesDoesNotAllowForbidden()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.StringRestrictions)));
            const string someValue = SomeRestrictedStringValue;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }
        
        [Test]
        public void EnumPropertyWithForbiddenValuesDoesNotAllowForbidden()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.EnumRestrictions)));
            const ClassWithForbiddenValues.SomeEnum someValue = ClassWithForbiddenValues.SomeEnum.A;
            toTest.Value = someValue;

            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }
        
        [Test]
        public void IntPropertyWithForbiddenGetsInitializedWithANonForbiddenValue()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.IntRestrictions)));

            Assert.That(toTest.Value, Is.Not.EqualTo(0));
        }
        
        [Test]
        public void StringPropertyWithForbiddenValuesGetsInitializedWithANonForbiddenValue()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.StringRestrictions)));

            Assert.That(toTest.Value, Is.Not.EqualTo(SomeRestrictedStringValue));
        }
        
        [Test]
        public void EnumPropertyWithForbiddenValuesGetsInitializedWithANonForbiddenValue()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.EnumRestrictions)));

            Assert.That(toTest.Value, Is.Not.EqualTo(ClassWithForbiddenValues.SomeEnum.A));
        }

        
        [Test]
        public void StringWithForbiddenNullValueShouldResultInFirstPredefinedValue()
        {
            var result = Produce<ClassWithForbiddenValues>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithForbiddenValues.NonNullString)));

            Assert.That(toTest.Value, Is.Not.EqualTo(null));
        }
    }
}