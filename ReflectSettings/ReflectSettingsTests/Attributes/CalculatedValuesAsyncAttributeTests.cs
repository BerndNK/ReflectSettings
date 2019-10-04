using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    internal class CalculatedValuesAsyncAttributeTests : EditableConfigFactoryTestBase
    {
        private const string SomeRestrictedValue = nameof(SomeRestrictedValue);
        private const string SomeRestrictedValue2 = nameof(SomeRestrictedValue2);

        // instantiated through base class
        // ReSharper disable once ClassNeverInstantiated.Local
        private class ClassWithCalculatedValuesAsync
        {
            [CalculatedValuesAsync(nameof(IntRestrictionsValues))]
            [UsedImplicitly]
            public int IntRestrictions { get; set; }

            [UsedImplicitly]
            public async Task<IEnumerable<object>> IntRestrictionsValues()
            {
                while (CalculationBlock) await Task.Delay(50);

                return new List<object> {5, 10};
            }

            [IgnoredForConfig] public bool CalculationBlock { get; set; } = false;

            [CalculatedValuesAsync(nameof(StringRestrictionsValues))]
            [UsedImplicitly]
            public string StringRestrictions { get; set; }

            [UsedImplicitly]
            public async Task<IEnumerable<object>> StringRestrictionsValues()
            {
                while (CalculationBlock) await Task.Delay(50);

                return new List<object> {SomeRestrictedValue};
            }

            [CalculatedValuesAsync(nameof(EnumRestrictionsValues))]
            [UsedImplicitly]
            public SomeEnum EnumRestrictions { get; set; }

            [UsedImplicitly]
            public async Task<IEnumerable<object>> EnumRestrictionsValues()
            {
                while (CalculationBlock) await Task.Delay(50);

                return new List<object> {SomeEnum.A, SomeEnum.B};
            }

            internal enum SomeEnum
            {
                A,
                B,
                C
            }
        }

        private class ClassWithAsyncChangingValues
        {
            [CalculatedValuesAsync(nameof(IntRestrictionsValues))]
            [UsedImplicitly]
            public int IntRestrictions { get; set; }

            [UsedImplicitly]
            public async Task<IEnumerable<object>> IntRestrictionsValues()
            {
                await Task.Delay(500);
                if (ReturnsTen)
                    return new List<object> {10};
                else
                    return new List<object> {5};
            }

            public bool ReturnsTen { get; set; }
        }
        
        private class ClassWithAsyncChangingValuesString
        {
            [CalculatedValuesAsync(nameof(StringRestrictionsValues))]
            [UsedImplicitly]
            public string StringRestrictions { get; set; }

            [UsedImplicitly]
            public async Task<IEnumerable<object>> StringRestrictionsValues()
            {
                await Task.Delay(500);
                if (ReturnsRestrictedValue2)
                    return new List<object> {SomeRestrictedValue2};
                else
                    return new List<object> {SomeRestrictedValue};
            }

            public bool ReturnsRestrictedValue2 { get; set; }
        }

        private async Task WaitWithTimeout(Task taskToWaitFor, int timeOutInMs)
        {
            var timeoutTask = Task.Delay(timeOutInMs);
            await Task.WhenAny(taskToWaitFor, timeoutTask);
        }

        private async Task WaitUntilNotBusy(IEditableConfig config)
        {
            while (config.IsBusy)
                await Task.Delay(10);
        }

        [Test]
        public async Task IntPropertyWithCalculatedValuesAsyncDoesAllowCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>(out var instance);

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.IntRestrictions)));
            instance.CalculationBlock = true;
            const int someValue = 10;
            toTest.Value = someValue;
            Assert.That(toTest.IsBusy, Is.True);
            instance.CalculationBlock = false;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1000);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public async Task IntPropertyWithCalculatedValuesAsyncDoesNotAllowNotCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>(out var instance);

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.IntRestrictions)));
            instance.CalculationBlock = true;
            const int someValue = 12;
            toTest.Value = someValue;
            instance.CalculationBlock = false;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1000);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void IntPropertyWithCalculatedValuesAsyncHasCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.IntRestrictions)));

            Assert.That(toTest.HasPredefinedValues, Is.True);
        }

        [Test]
        public async Task IntPropertyWithCalculatedValuesAsyncDoesNotChangeUntilCalculationFinished()
        {
            var result = Produce<ClassWithAsyncChangingValues>(out var instance).ToList();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValues.IntRestrictions)));
            const int someValue = 10;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1500);
            var calculatedValuesChangingProperty = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValues.ReturnsTen)));
            calculatedValuesChangingProperty.Value = true;
            calculatedValuesChangingProperty.Value = false;
            Assert.That(toTest.IsBusy, Is.True);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
            toTest.Value = someValue;
            Assert.That(toTest.Value, Is.EqualTo(someValue));
            Assert.That(toTest.IsBusy, Is.True);

            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1500);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public async Task IntPropertyWithCalculatedValuesAsyncDoesChangeAfterCalculationFinished()
        {
            var result = Produce<ClassWithAsyncChangingValues>(out var instance);

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValues.IntRestrictions)));
            const int someValue = 10;
            instance.ReturnsTen = true;
            toTest.Value = someValue;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1500);

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public async Task StringPropertyWithCalculatedValuesAsyncDoesAllowCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>(out var instance);

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.IntRestrictions)));
            instance.CalculationBlock = true;
            const int someValue = 10;
            toTest.Value = someValue;
            Assert.That(toTest.IsBusy, Is.True);
            instance.CalculationBlock = false;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1000);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }

        [Test]
        public async Task StringPropertyWithCalculatedValuesAsyncDoesNotAllowNotCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>(out var instance);

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.StringRestrictions)));
            instance.CalculationBlock = true;
            const string someValue = "anything";
            toTest.Value = someValue;
            instance.CalculationBlock = false;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1000);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public void StringPropertyWithCalculatedValuesAsyncHasCalculatedValue()
        {
            var result = Produce<ClassWithCalculatedValuesAsync>();

            var toTest = result.First(x =>
                x.PropertyInfo.Name.Equals(nameof(ClassWithCalculatedValuesAsync.StringRestrictions)));

            Assert.That(toTest.HasPredefinedValues, Is.True);
        } 

        [Test]
        public async Task StringPropertyWithCalculatedValuesAsyncDoesNotChangeUntilCalculationFinished()
        {
            var result = Produce<ClassWithAsyncChangingValuesString>(out var instance).ToList();

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValuesString.StringRestrictions)));
            const string someValue = "someValue";
            var calculatedValuesChangingProperty = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValuesString.ReturnsRestrictedValue2)));
            calculatedValuesChangingProperty.Value = true;
            calculatedValuesChangingProperty.Value = false;
            Assert.That(toTest.IsBusy, Is.True);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
            toTest.Value = someValue;
            Assert.That(toTest.Value, Is.EqualTo(someValue));
            Assert.That(toTest.IsBusy, Is.True);

            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1500);

            Assert.That(toTest.IsBusy, Is.False);
            Assert.That(toTest.Value, Is.Not.EqualTo(someValue));
        }

        [Test]
        public async Task StringPropertyWithCalculatedValuesAsyncDoesChangeAfterCalculationFinished()
        {
            var result = Produce<ClassWithAsyncChangingValuesString>(out var instance);

            var toTest = result.First(x => x.PropertyInfo.Name.Equals(nameof(ClassWithAsyncChangingValuesString.StringRestrictions)));
            const string someValue = SomeRestrictedValue2;
            instance.ReturnsRestrictedValue2 = true;
            toTest.Value = someValue;
            await WaitWithTimeout(WaitUntilNotBusy(toTest), 1500);

            Assert.That(toTest.Value, Is.EqualTo(someValue));
        }
    }
}