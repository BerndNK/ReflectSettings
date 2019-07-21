using System.Collections.Generic;
using JetBrains.Annotations;
using ReflectSettings.Factory.Attributes;

namespace FrontendDemo
{
    internal class ComplexConfiguration
    {
        [UsedImplicitly]
        public string Username { get; set; }

        [PredefinedValues("Attack helicopter", "I rather not say", "Why is this even important?")]
        [UsedImplicitly]
        public string Gender { get; set; }
        
        [UsedImplicitly]
        public ApplicationMode ApplicationMode { get; set; }
        
        [UsedImplicitly]
        public List<Curreny> Currencies { get; set; }

        [CalculatedValues(nameof(ActiveCurrencyPossibleValues))]
        [UsedImplicitly]
        public Curreny ActiveCurrency { get; set; }
        
        [UsedImplicitly]
        public IEnumerable<object> ActiveCurrencyPossibleValues() => Currencies;
    }

    internal enum ApplicationMode
    {
        Default,
        Turbo,
        Debug
    }

    internal class Curreny
    {
        public string Name { get; set; }

        [MinMax]
        public double DollarToCurrencyFactory { get; set; }
    }
}
