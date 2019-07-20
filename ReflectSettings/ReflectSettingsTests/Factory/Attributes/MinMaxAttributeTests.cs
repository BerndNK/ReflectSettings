using System.Linq;
using NUnit.Framework;
using ReflectSettings.Factory.Attributes;

namespace ReflectSettingsTests.Factory.Attributes
{
    [TestFixture]
    class MinMaxAttributeTests : EditableConfigFactoryTestBase
    {
        private class ClassWithNumbers
        {
            [MinMax(20, 25)]
            public int IntProperty { get; set; }

            [MinMax(-30.0, double.MaxValue)]
            public double NegativeMin { get; set; }

            [MinMax(10.0, 5.0)]
            public double IllegalMinMax { get; set; }

        }

        public void HigherMinThanMaxGetsTreatedAsMax()
        {
            var result = Produce<ClassWithNumbers>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IllegalMinMax)));
            toTest.Value = 20.0;

            // 10.0 was given as the minimum but should be treated as the maximum instead, as the given maximum was smaller
            Assert.That(toTest.Value, Is.EqualTo(10.0));
        }
        
        public void LowerMaxThanMinGetsTreatedAsMin()
        {
            var result = Produce<ClassWithNumbers>();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithNumbers.IllegalMinMax)));
            toTest.Value = 2;

            // 5.0 was given as the maximum but should be treated as the minimum instead, as the given maximum was smaller
            Assert.That(toTest.Value, Is.EqualTo(5.0));
        }
    }
}
