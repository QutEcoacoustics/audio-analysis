// <copyright file="Filter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace AcousticWorkbench
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Acoustics.Shared;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial record QueryFilter(Projection Projection, ImmutableDictionary<string, object> Filter, Sorting Sorting);

    public partial record QueryFilter
    {
        public static QueryFilter Empty { get; } = new(new(), ImmutableDictionary<string, object>.Empty, null);
    }

    public record Projection(ImmutableArray<string>? Include = default, ImmutableArray<string>? Exclude = default);

    public record Sorting(string OrderBy, Direction Direction);

    [Serializable]
    [JsonConverter(typeof(StringEnumConverter))]
    public enum Direction
    {
        Desc,
        Asc,
    }

    public static class FilterExtensions
    {
        public static QueryFilter WithProjectionInclude(this QueryFilter filter, params string[] fields)
        {
            return filter with
            {
                Projection = filter.Projection with
                {
                    Include = ApiNaming(fields)
                }
            };
        }

        public static QueryFilter WithProjectionExclude(this QueryFilter filter, params string[] fields)
        {
            return filter with
            {
                Projection = filter.Projection with
                {
                    Exclude = ApiNaming(fields)
                }
            };
        }

        public static QueryFilter FilterById(this QueryFilter filter, long id, string name = "id")
        {
            var result = filter.Filter.Add(
                    ApiNaming(name),
                    Pairs(("eq", id)));

            return filter with
            {
                Filter = result
            };
        }

        public static QueryFilter FilterByIds(this QueryFilter filter, string name = "id", params ulong[] ids)
        {
            if (ids is null or { Length: 0 })
            {
                return filter;
            }

            var result = filter.Filter.Add(
                    ApiNaming(name),
                    Pairs(("in", ids)));

            return filter with
            {
                Filter = result
            };
        }

        public static QueryFilter FilterByRange<T>(this QueryFilter filter, string field, Interval<T>? range)
            where T : struct, IComparable<T>, IFormattable
        {
            if (range is null or { IsEmpty: true })
            {
                return filter;
            }

            var result = filter.Filter.Add(
                    ApiNaming(field),
                    Pairs(
                        ("range",
                         Pairs(
                            (ApiNaming("interval"), range.Value.ToString(true))))));

            return filter with
            {
                Filter = result
            };
        }

        public static QueryFilter OrderBy(this QueryFilter filter, string field, Direction direction = Direction.Asc)
        {
            return filter with
            {
                Sorting = new Sorting(ApiNaming(field), direction)
            };
        }

        private static ImmutableDictionary<string, object> Pairs(params (string Key, object Value)[] pairs)
        {
            return pairs.ToImmutableDictionary(pairs => pairs.Key, pairs => pairs.Value);
        }

        private static ImmutableArray<string> ApiNaming(string[] names)
        {
            return names.Select(x => Service.NamingStrategy.GetPropertyName(x, hasSpecifiedName: false)).ToImmutableArray();
        }

        private static string ApiNaming(string name)
        {
            return Service.NamingStrategy.GetPropertyName(name, hasSpecifiedName: false);
        }
    }
}
