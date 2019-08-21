using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    [TestFixture]
    internal class EditableComplexTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithComplexProperty
        {
            public SubClass SubClass { get; set; }
        }
        
        [UsedImplicitly]
        private class SubClass
        {
            public string SomeString { get; set; }
        }
        
        [UsedImplicitly]
        private class ClassWithAllowedNullComplexProperty
        {
            [PredefinedValues(null)]
            [UsedImplicitly]
            public SubClass SubClass { get; set; }
        }

        [Test]
        public void InstanceWithComplexPropertyShouldHaveNotNullProperty()
        {
            var result = Produce<ClassWithComplexProperty>();
            var complexProperty = result.First(c => c.PropertyInfo.Name == nameof(ClassWithComplexProperty.SubClass)) as EditableComplex<SubClass>;

            var subClass = complexProperty?.Value as SubClass;

            Assert.NotNull(subClass);
        }
        
        [Test]
        public void ComplexPropertyHasSubEditables()
        {
            var result = Produce<ClassWithComplexProperty>(out var instance);
            var complexProperty = result.First(c => c.PropertyInfo.Name == nameof(ClassWithComplexProperty.SubClass)) as EditableComplex<SubClass>;

            var subClass = complexProperty?.Value as SubClass;
            if(subClass is null || complexProperty is null)
                Assert.Fail();

            var stringProperty = complexProperty.SubEditables.First(x => x.PropertyInfo.Name == nameof(SubClass.SomeString));
            const string someValue = nameof(someValue);
            stringProperty.Value = someValue;

            Assert.That(instance.SubClass.SomeString, Is.EqualTo(someValue));
        }
        
        [Test]
        public void ComplexPropertyThatMayBeNullIsNull()
        {
            Produce<ClassWithAllowedNullComplexProperty>(out var instance);
            
            Assert.That(instance.SubClass, Is.EqualTo(null));
        }

        [Test]
        public void SubEditablesShouldReceiveAdditionalDataFromParent()
        {
            var result = Produce<ClassWithComplexProperty>();
            var complexProperty = (EditableComplex<SubClass>) result.First(c => c.PropertyInfo.Name == nameof(ClassWithComplexProperty.SubClass));
            complexProperty.AdditionalData = new { };

            Assert.That(complexProperty.SubEditables.All(x => x.AdditionalData == complexProperty.AdditionalData), Is.True);
        }
    }
}
