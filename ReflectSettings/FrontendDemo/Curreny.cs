using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using ReflectSettings.Attributes;

namespace FrontendDemo
{
    internal class Curreny : INotifyPropertyChanged
    {
        private string _name;
        private double _dollarToCurrencyFactory;

        [IsDisplayName]
        public string Name
        {
            get => _name;
            set
            {
                _name = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string DisplayName => $"{(string.IsNullOrWhiteSpace(Name) ? $"[{nameof(Curreny)}]" : Name)} 1$ = {DollarToCurrencyFactory}{Name}";

        [MinMax]
        public double DollarToCurrencyFactory
        {
            get => _dollarToCurrencyFactory;
            set
            {
                _dollarToCurrencyFactory = value; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public override string ToString() => DisplayName;
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override bool Equals(object obj)
        {
            var otherAsCurrency = obj as Curreny;
            if (otherAsCurrency == null)
                return false;
            return Name == otherAsCurrency.Name && DollarToCurrencyFactory == otherAsCurrency.DollarToCurrencyFactory;
        }
    }
}