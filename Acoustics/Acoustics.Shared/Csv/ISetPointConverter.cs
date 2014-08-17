// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SetPointConverter.cs" company="QutBioacoustics">
//   All code in this file and all associated files are the copyright of the QUT Bioacoustics Research Group (formally MQUTeR).
// </copyright>
// <summary>
//   Defines the ISetPointConverter type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Acoustics.Shared.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Drawing;
    using System.Linq;
    using System.Text;

    using CsvHelper.TypeConversion;

    public class CsvSetPointConverter : ITypeConverter
    {
        public const string InnerFieldDelimitter = "*";
        public const string InnerItemDelimitter = ";";

        public string ConvertToString(TypeConverterOptions options, object value)
        {
            Contract.Requires(value == null || value is ISet<Point>);

            if (value == null)
            {
                return string.Empty;
            }

            var set = (ISet<Point>)value;

            if (set.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder(set.Count * (4 + 1 + 4 + 1));

            foreach (var point in set)
            {
                sb.Append(point.X.ToString(options.CultureInfo.NumberFormat));
                sb.Append(InnerFieldDelimitter);
                sb.Append(point.Y.ToString(options.CultureInfo.NumberFormat));
                sb.Append(InnerItemDelimitter);
            }

            return sb.ToString();
        }

        public object ConvertFromString(TypeConverterOptions options, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var items = text.Split(
                new[] { InnerFieldDelimitter, InnerItemDelimitter },
                StringSplitOptions.RemoveEmptyEntries);

            var result = new HashSet<Point>();
            for (int i = 0; i < items.Length; i += 2)
            {
                var a = int.Parse(items[i], options.NumberStyle.GetValueOrDefault());
                var b = int.Parse(items[i + 1], options.NumberStyle.GetValueOrDefault());
                result.Add(new Point(a, b));
            }

            return result;
        }

        public bool CanConvertFrom(Type type)
        {
            return type == typeof(string);
        }

        public bool CanConvertTo(Type type)
        {
            return type == typeof(string);
        }
    }
}
