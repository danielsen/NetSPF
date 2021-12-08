using System;
using System.Linq;

namespace NetSPF.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class MappedTextAttribute : Attribute
    {
        public MappedTextAttribute(string name)
        {
            if (name == null)
                throw new ArgumentNullException();

            Names = new[] {name};
        }

        public MappedTextAttribute(params string[] names)
        {
            if (names == null || names.Any(x => x == null))
                throw new ArgumentNullException();

            Names = names;
        }

        public string[] Names { get; } 
    }
}