using System.Collections.Generic;
using JetBrains.Annotations;
using ReflectSettings.Attributes;

namespace FrontendDemo
{
    [UsedImplicitly]
    public class RestrictedStringList
    {
        [TypesForInstantiation(typeof(List<string>))]
        [CalculatedValues(nameof(ComplexConfiguration.AllowedStringsForRestrictedList), true)]
        public IList<string> SelectedStrings { get; set; }
    }
}
