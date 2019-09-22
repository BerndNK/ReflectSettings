using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using NUnit.Framework;
using ReflectSettings.EditableConfigs;

namespace ReflectSettingsTests.EditableConfigs
{
    internal class EditableBoolTests : EditableConfigFactoryTestBase
    {
        [UsedImplicitly]
        private class ClassWithBoolProperties
        {
            [UsedImplicitly]
            public bool AllowAnything { get; set; }
        }

        private class ClassRaisingPropertyChanged : INotifyPropertyChanged
        {
            private bool _someBool;

            public bool SomeBool
            {
                get => _someBool;
                set
                {
                    _someBool = value;
                    OnPropertyChanged();
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Test]
        public void ClassWithBoolPropertyShouldResultInEditableBol()
        {
            var result = Produce<ClassWithBoolProperties>(out var instance);

            var boolEditable = result.OfType<EditableBool>().FirstOrDefault();

            Assert.That(boolEditable, Is.Not.Null);
        }
        
        [Test]
        public void ChangingPropertyRaisesPropertyChanged()
        {
            var result = Produce<ClassRaisingPropertyChanged>(out var instance);

            var boolEditable = result.OfType<EditableBool>().First();
            var propertyChangedRaised = false;
            boolEditable.PropertyChanged += (sender, args) => propertyChangedRaised = true;

            instance.SomeBool = true;

            Assert.That(propertyChangedRaised, Is.True);
        }
    }
}
