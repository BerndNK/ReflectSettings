using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings;
using ReflectSettings.Attributes;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.Attributes
{
    internal class CalculatedTypeAttributeTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithSpecifiedTypeAsString
        {
            [CalculatedType(nameof(CalculateTypes))]
            public object CanBeAnything { get; set; }

            public Type CalculateTypes(ClassWithSpecifiedTypeAsString instance)
            {
                return typeof(string);
            }
        }

        [UsedImplicitly]
        private class ClassWithSpecifiedTypeAsComplex
        {
            [CalculatedType(nameof(CalculateTypes))]
            public object CanBeAnything { get; set; }

            public Type CalculateTypes(ClassWithSpecifiedTypeAsComplex instance)
            {
                return typeof(ComplexSubClass);
            }
        }

        [UsedImplicitly]
        private class ClassWithVariableSpecifiedType
        {
            public bool CanBeAnythingIsComplex { get; set; }

            [CalculatedType(nameof(CalculateTypes))]
            public object CanBeAnything { get; set; }

            public Type CalculateTypes(ClassWithVariableSpecifiedType instance)
            {
                if (CanBeAnythingIsComplex)
                    return typeof(ComplexSubClass);
                else
                    return typeof(string);
            }
        }

        [UsedImplicitly]
        private class ClassWithSpecifiedTypeFromParent
        {
            [CalculatedType(nameof(ClassWhichTellsItsChildrenWhichTypeTheyAre.CalculateTypes))]
            public object CanBeAnything { get; set; }
        }

        [UsedImplicitly]
        private class ClassWhichTellsItsChildrenWhichTypeTheyAre
        {
            [TypesForInstantiation(typeof(List<ClassWithSpecifiedTypeFromParent>))]
            [CalculatedType(nameof(CalculateTypes), nameof(CalculateTypes))]
            public IList<ClassWithSpecifiedTypeFromParent> Children { get; set; }

            public Type CalculateTypes(ClassWithSpecifiedTypeFromParent instance)
            {
                // the first instance is always a string, while everyone else should be complex
                if (Children.IndexOf(instance) == 0 || Children.Count == 0)
                    return typeof(string);
                else
                    return typeof(ComplexSubClass);
            }
        }


        [UsedImplicitly]
        private class ClassWithListWhichFirstInstanceIsStringAndOthersAreComplex
        {
            [TypesForInstantiation(typeof(List<object>))]
            [CalculatedType(nameof(CalculateTypes), true)]
            public IList<object> Children { get; set; }

            public Type CalculateTypes(object instance)
            {
                // the first instance is always a string, while everyone else should be complex
                if (Children == null || Children.IndexOf(instance) == 0 || Children.Count == 0)
                    return typeof(string);
                else
                    return typeof(ComplexSubClass);
            }
        }

        private interface ISomething
        {
        }

        private class Something : ISomething
        {
        }

        [UsedImplicitly]
        private class ClassWithWrongSpecifiedType
        {
            [CalculatedType(nameof(CalculateTypes))]
            [CalculatedType(nameof(CalculateTypes2))]
            public ISomething CanOnlyBeISomething { get; set; }

            public Type CalculateTypes(ClassWithWrongSpecifiedType instance)
            {
                return typeof(string);
            }

            public Type CalculateTypes2(ClassWithWrongSpecifiedType instance)
            {
                return typeof(Something);
            }
        }

        private class ComplexSubClass
        {
            public int SomeInt { get; set; } = 5;
        }

        [Test]
        public void PropertyGetsInitializedWithEmptyStringForCalculatedStringType()
        {
            var result = Produce<ClassWithSpecifiedTypeAsString>(out var instance);

            Assert.That(instance.CanBeAnything, Is.EqualTo(""));
        }

        [Test]
        public void PropertyGetSetToComplexTypeIfTypeIsSpecified()
        {
            var result = Produce<ClassWithSpecifiedTypeAsComplex>(out var instance);

            Assert.That(instance.CanBeAnything, Is.TypeOf(typeof(ComplexSubClass)));
        }

        [Test]
        public void PropertyWithCalculatedTypeGetsInitializedWithDefaultConstructorOfType()
        {
            var result = Produce<ClassWithSpecifiedTypeAsComplex>(out var instance);

            var complexSubClass = (ComplexSubClass) instance.CanBeAnything;

            Assert.That(complexSubClass.SomeInt, Is.EqualTo(5));
        }

        [Test]
        public void PropertyWithCalculatedTypeChangesTypeWhenCalculatedValueChanges()
        {
            var result = Produce<ClassWithVariableSpecifiedType>(out var instance);
            var boolEditable = result.OfType<EditableBool>().First(x =>
                x.PropertyInfo.Name == nameof(ClassWithVariableSpecifiedType.CanBeAnythingIsComplex));

            boolEditable.Value = true;

            Assert.That(instance.CanBeAnything, Is.TypeOf(typeof(ComplexSubClass)));
        }

        [Test]
        public void MultipleEditablesOfSameTypeUpdateTheirSubEditables()
        {
            var result = Produce<ClassWithVariableSpecifiedType>(out var instance).ToList();

            var factory = new SettingsFactory();
            var doubleReflected = factory.Reflect(instance, result.First().ChangeTrackingManager).ToList();
            
            var boolEditable = result.OfType<EditableBool>().First(x =>
                x.PropertyInfo.Name == nameof(ClassWithVariableSpecifiedType.CanBeAnythingIsComplex));

            var reflectedAnything = result.OfType<IEditableComplex>().First();
            var reflectedAnything2 = doubleReflected.OfType<IEditableComplex>().First();

            boolEditable.Value = true;

            Assert.That(reflectedAnything.SubEditables.Any(x => x is EditableInt), Is.True);
            Assert.That(reflectedAnything2.SubEditables.Any(x => x is EditableInt), Is.True);
        }

        [Test]
        public void CalculatedTypeWorksForChildrenViaKey()
        {
            var result = Produce<ClassWhichTellsItsChildrenWhichTypeTheyAre>(out var instance);
            var listEditable = result.OfType<IEditableCollection>().First(x => x.PropertyInfo.Name == nameof(ClassWhichTellsItsChildrenWhichTypeTheyAre.Children));

            listEditable.AddNewItemCommand.Execute(null);
            listEditable.AddNewItemCommand.Execute(null);

            Assert.That(instance.Children.First().CanBeAnything, Is.TypeOf(typeof(string)));
            Assert.That(instance.Children.Last().CanBeAnything, Is.TypeOf(typeof(ComplexSubClass)));
        }

        [Test]
        public void CalculatedTypeWorksForListEntries()
        {
            var result = Produce<ClassWithListWhichFirstInstanceIsStringAndOthersAreComplex>(out var instance);
            var listEditable = result.OfType<IEditableCollection>().First(x => x.PropertyInfo.Name == nameof(ClassWithListWhichFirstInstanceIsStringAndOthersAreComplex.Children));

            listEditable.AddNewItemCommand.Execute(null);
            listEditable.AddNewItemCommand.Execute(null);

            Assert.That(instance.Children.First(), Is.TypeOf(typeof(string)));
            Assert.That(instance.Children.Last(), Is.TypeOf(typeof(ComplexSubClass)));
        }

        [Test]
        public void NonMatchingTypesGetIgnored()
        {
            var result = Produce<ClassWithWrongSpecifiedType>(out var instance);

            Assert.That(instance.CanOnlyBeISomething, Is.TypeOf(typeof(Something)));
        }
    }
}