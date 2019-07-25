using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using ReflectSettings.Attributes;

namespace FrontendDemo
{
    internal class ComplexConfiguration
    {/*
        [UsedImplicitly]
        public string Username { get; set; }

        [PredefinedValues("Attack helicopter", "I rather not say", "Why is this even important?")]
        [UsedImplicitly]
        public string Gender { get; set; }
        
        [UsedImplicitly]
        public ApplicationMode ApplicationMode { get; set; }
        
        [UsedImplicitly]
        public ObservableCollection<Curreny> Currencies { get; set; }

        [CalculatedValues(nameof(ActiveCurrencyPossibleValues))]
        [UsedImplicitly]
        public Curreny ActiveCurrency { get; set; }
        
        [UsedImplicitly]
        public IEnumerable<object> ActiveCurrencyPossibleValues() => Currencies;
        
        [UsedImplicitly]
        public bool UseLightTheme { get; set; }

        [TypesForInstantiation(typeof(Dictionary<string,string>))]
        public IReadOnlyDictionary<string, string> SomeDictionary { get; set; }

        public Curreny SomeCurrencyThatJustIsHereForDemonstrationPurposes { get; set; }

        public CultureInfo CultureInfo { get; set; }*/

        [TypesForInstantiation(typeof(List<Curreny>))]
        public IList<Curreny> AllowedStringsForSubInstances { get; set; }

        public IEnumerable<string> AllowedStringsForRestrictedList() => AllowedStringsForSubInstances.Select(x => x.DisplayName);

        [TypesForInstantiation(typeof(List<RestrictedStringList>))]
        [CalculatedValues(nameof(AllowedStringsForRestrictedList), nameof(AllowedStringsForRestrictedList))]
        public IList<RestrictedStringList> SubInstancesWithRestrictedList { get; set; }
    }
}
