using System;
using System.Collections.Generic;
using System.Text;
using ReflectSettings.Factory.Attributes;

namespace FrontendDemo
{
    class ComplexConfiguration
    {
        public string Username { get; set; }

        [PredefinedValues("Attack helicopter", "I rather not say", "Why is this even important?")]
        public string Gender { get; set; }

        public ApplicationMode ApplicationMode { get; set; }

        public List<Curreny> Currencies { get; set; }

        [CalculatedValues(nameof(ActiveCurrencyPossibleValues))]
        public Curreny ActiveCurrency { get; set; }

        public IEnumerable<object> ActiveCurrencyPossibleValues() => Currencies;
    }

    enum ApplicationMode
    {
        Default,
        Turbo,
        Debug
    }

    class Curreny
    {
        public string Name { get; set; }

        [MinMax]
        public double DollarToCurrencyFactory { get; set; }
    }
}
