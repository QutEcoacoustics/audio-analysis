// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopologyEnumConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Csv
{
    using System;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    public class TopologyEnumConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (Enum.TryParse<Topology>(text, ignoreCase: true, out var value))
            {
                return value;
            }
            else
            {
                return base.ConvertFromString(text, row, memberMapData);
            }
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            return value switch
            {
                Topology.Open => nameof(Topology.Exclusive),
                Topology.LeftClosedRightOpen => nameof(Topology.MinimumInclusiveMaximumExclusive),
                Topology.LeftOpenRightClosed => nameof(Topology.MinimumExclusiveMaximumInclusive),
                Topology.Closed => nameof(Topology.Inclusive),
                _ => throw new ArgumentException($"`{value}` is not a valid Interval Topology"),
            };
        }
    }
}