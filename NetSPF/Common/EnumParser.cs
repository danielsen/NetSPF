using System.Collections.Generic;
using System.Linq;
using NetSPF.Common.Attributes;

namespace NetSPF.Common
{
    internal static class EnumParser
    {
        public static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
            return ParseEnumImpl<TEnum>.Values[value.ToLower()];
        }

        public static bool TryParse<TEnum>(string value, out TEnum result) where TEnum : struct
        {
            return ParseEnumImpl<TEnum>.Values.TryGetValue(value.ToLower(), out result);
        }

        private static class ParseEnumImpl<TEnum> where TEnum : struct
        {
            public static readonly Dictionary<string, TEnum> Values = new Dictionary<string, TEnum>();

            static ParseEnumImpl()
            {
                var fieldNames = typeof(TEnum).GetFields()
                    .Select(x => new
                    {
                        Value = x,
                        Names = (IEnumerable<MappedTextAttribute>) new[] {new MappedTextAttribute(x.Name)}
                    }).Where(x => x.Value.Name != "value__").ToList();
                var nameAttributes = typeof(TEnum)
                    .GetFields()
                    .Select(x => new
                    {
                        Value = x,
                        Names = x.GetCustomAttributes(typeof(MappedTextAttribute), false)
                            .Cast<MappedTextAttribute>()

                    }).ToList();
                var allNames = nameAttributes.Concat(fieldNames);

                var degrouped = allNames.SelectMany(
                    x => x.Names.SelectMany(y => y.Names), 
                    (x, y) => new { Value = x.Value, Name = y });

                Values = degrouped.ToDictionary(
                    x => x.Name.ToLower(), 
                    x => (TEnum)x.Value.GetValue(null));
            }
        } 
    }
}