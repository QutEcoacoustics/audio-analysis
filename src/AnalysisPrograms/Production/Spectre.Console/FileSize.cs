// <copyright file="FileSize.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace Spectre.Console
{
    using System;
    using System.Globalization;

    internal enum FileSizeUnit
    {
        Byte = 0,
        KiloByte = 1,
        MegaByte = 2,
        GigaByte = 3,
        TeraByte = 4,
        PetaByte = 5,
        ExaByte = 6,
        ZettaByte = 7,
        YottaByte = 8,
    }

    // https://raw.githubusercontent.com/spectreconsole/spectre.console/a690ce49556615fea49e61972646eb52a11bbdb5/src/Spectre.Console/Internal/FileSize.cs
    internal struct FileSize
    {
        public double Bytes { get; }

        public FileSizeUnit Unit { get; }

        public string Suffix => this.GetSuffix();

        public FileSize(double bytes)
        {
            this.Bytes = bytes;
            this.Unit = Detect(bytes);
        }

        public FileSize(double bytes, FileSizeUnit unit)
        {
            this.Bytes = bytes;
            this.Unit = unit;
        }

        public string Format(CultureInfo? culture = null)
        {
            var @base = GetBase(this.Unit);
            if (@base == 0)
            {
                @base = 1;
            }

            var bytes = this.Bytes / @base;

            return this.Unit == FileSizeUnit.Byte
                ? ((int)bytes).ToString(culture ?? CultureInfo.InvariantCulture)
                : bytes.ToString("F1", culture ?? CultureInfo.InvariantCulture);
        }

        public override string ToString()
        {
            return this.ToString(suffix: true, CultureInfo.InvariantCulture);
        }

        public string ToString(bool suffix = true, CultureInfo? culture = null)
        {
            if (suffix)
            {
                return $"{this.Format(culture)} {this.Suffix}";
            }

            return this.Format(culture);
        }

        private string GetSuffix()
        {
            return (this.Bytes, this.Unit) switch
            {
                (_, FileSizeUnit.KiloByte) => "KB",
                (_, FileSizeUnit.MegaByte) => "MB",
                (_, FileSizeUnit.GigaByte) => "GB",
                (_, FileSizeUnit.TeraByte) => "TB",
                (_, FileSizeUnit.PetaByte) => "PB",
                (_, FileSizeUnit.ExaByte) => "EB",
                (_, FileSizeUnit.ZettaByte) => "ZB",
                (_, FileSizeUnit.YottaByte) => "YB",
                (1, _) => "byte",
                (_, _) => "bytes",
            };
        }

        private static FileSizeUnit Detect(double bytes)
        {
            foreach (var unit in (FileSizeUnit[])Enum.GetValues(typeof(FileSizeUnit)))
            {
                if (bytes < (GetBase(unit) * 1024))
                {
                    return unit;
                }
            }

            return FileSizeUnit.Byte;
        }

        private static double GetBase(FileSizeUnit unit)
        {
            return Math.Pow(1024, (int)unit);
        }
    }
}
