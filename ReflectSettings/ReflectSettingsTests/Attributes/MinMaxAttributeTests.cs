using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;

namespace ReflectSettingsTests.Attributes
{
    [TestFixture]
    internal class MinMaxAttributeTests : EditableConfigFactoryTestBase
    {
        private class ClassWithNumbers
        {
            [MinMax(20, 25)]
            [UsedImplicitly]
            public int IntProperty { get; set; }

            [MinMax(-30.0, double.MaxValue)]
            [UsedImplicitly]
            public double NegativeMin { get; set; }

            [MinMax(10.0, 5.0)]
            [UsedImplicitly]
            public double IllegalMinMax { get; set; }

        }

        [Test]
        public void HigherMinThanMaxGetsTreatedAsMax()
        {
            var result = Produce<ClassWithNumbers>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IllegalMinMax)));
            toTest.Value = 20.0;

            // 10.0 was given as the minimum but should be treated as the maximum instead, as the given maximum was smaller
            Assert.That(toTest.Value, Is.EqualTo(10.0));
        }
        
        [Test]
        public void LowerMaxThanMinGetsTreatedAsMin()
        {
            var result = Produce<ClassWithNumbers>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IllegalMinMax)));
            toTest.Value = 2;

            // 5.0 was given as the maximum but should be treated as the minimum instead, as the given maximum was smaller
            Assert.That(toTest.Value, Is.EqualTo(5.0));
        }

        [Test]
        public void MinIsRespectedWhenSettingValue()
        {
            var result = Produce<ClassWithNumbers>();

            
            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IntProperty)));
            toTest.Value = 2;

            // 20 is the given minimum
            Assert.That(toTest.Value, Is.EqualTo(20));
        }

        [Test]
        public void MaxIsRespectedWhenSettingValue()
        {
            var result = Produce<ClassWithNumbers>();
            
            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IntProperty)));
            toTest.Value = 200;

            // 25 is the given minimum
            Assert.That(toTest.Value, Is.EqualTo(25));
        }

        [Test]
        public void PropertyIsInitializedWithMinValue()
        {
            var result = Produce<ClassWithNumbers>();
            
            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IntProperty)));

            // 20 is the given minimum
            Assert.That(toTest.Value, Is.EqualTo(20));
        }

        
        [Test]
        public void NegativeMinIsRespectedWhenSettingValue()
        {
            var result = Produce<ClassWithNumbers>();
            
            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.NegativeMin)));
            toTest.Value = -500;

            // -30 is the given minimum
            Assert.That(toTest.Value, Is.EqualTo(-30));
        }

    }
}
