// <copyright file="CsvIntervalConverter.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Acoustics.Shared.Csv
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Acoustics.Shared.Contracts;
    using CsvHelper;
    using CsvHelper.Configuration;
    using CsvHelper.TypeConversion;

    // reference implementation: https://github.com/JoshClose/CsvHelper/blob/3b14b70fd1e9ce742375fbb116799b19fd0e7ccd/src/CsvHelper/TypeConversion/DoubleConverter.cs
    public class CsvIntervalConverter : ITypeConverter
    {
        public object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            throw new NotImplementedException();
        }

        public string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
        {
            if (value is Interval<double> d)
            {
                var doubleOptions = row?.Context?.TypeConverterOptionsCache?.GetOptions<double>();
                return d.ToString(suppressName: true, doubleOptions?.Formats?.FirstOrDefault(), doubleOptions?.CultureInfo);
            }

            throw new InvalidOperationException("Cannot convert interval that is not have the generic type double");
        }
    }
}
