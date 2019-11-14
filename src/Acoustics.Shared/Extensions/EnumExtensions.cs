// <copyright file="EnumExtensions.cs" company="QutEcoacoustics">
// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
// </copyright>

namespace System
{
    using Acoustics.Shared;

    public static class EnumExtensions
    {
        public static ImageChrome ToImageChrome(this bool chromeOrNot) => chromeOrNot ? ImageChrome.With : ImageChrome.Without;

        public static string PrintEnumOptions(this Type @enum)
        {
            if (@enum == null || !@enum.IsEnum)
            {
                throw new ArgumentException($"{nameof(PrintEnumOptions)} must only be used on an enum type", nameof(@enum));
            }

            return Enum.GetValues(@enum).Join("|");
        }
    }
}