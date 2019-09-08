using System.Collections.Generic;
using System.Linq;
using ReflectSettings.Attributes;

namespace ReflectSettings.EditableConfigs.InheritingAttribute
{
    /// <summary>
    /// Includes attributes which are meant to be inheritable to sub editables. An attribute without a key, is meant for the attached instance.
    /// An attribute with a key, is meant as a lookup which sub editables can use.
    /// </summary>
    public class InheritedAttributes<T> where T : BaseKeyValueAttribute
    {
        private readonly IList<T> _attributesOfThis;

        public InheritedAttributes(IEnumerable<T> attributesOfThis)
        {
            _attributesOfThis = attributesOfThis.ToList();
        }

        /// <summary>
        /// All attributes which are inherited from parent editables.
        /// </summary>
        public IReadOnlyList<T> Inherited => _inherited;

        private List<T> _inherited = new List<T>();

        /// <summary>
        /// All attributes attached to the current property as well as all inherited ones.
        /// </summary>
        public IEnumerable<T> All => Inherited.Concat(_attributesOfThis);

        /// <summary>
        /// All attributes which shall be inherited by sub editables.
        /// </summary>
        public IEnumerable<T> ForChildren => All.Where(x => x.Key != null || x.ForCollectionEntries);

        /// <summary>
        /// All attributes which shall not be inherited and instead are meant for this instance.
        /// </summary>
        public IEnumerable<T> ForThis => All.Where(x => x.Key == null);

        public void InheritFrom(InheritedAttributes<T> parentAttributes)
        {
            // add all which are not already present
            _inherited.AddRange(parentAttributes.ForChildren.Except(Inherited));
        }
    }
}