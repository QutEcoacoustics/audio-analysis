// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParsingUtilities.cs" company="MQUTeR">
//   -
// </copyright>
// <summary>
//   Defines the ParsingUtilities type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace QutSensors.UI
{
    public static class ParsingUtilities
    {
        public static Guid? ParseGuid(string value)
        {
            if (string.IsNullOrEmpty(value) || value == Guid.Empty.ToString())
                return null;
            try
            {
                return new Guid(value);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static bool? ParseBool(string value)
        {
            bool temp;
            if (!string.IsNullOrEmpty(value) && bool.TryParse(value, out temp))
                return temp;
            return null;
        }

        public static int? ParseInt(string value)
        {
            int temp;
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out temp))
                return temp;
            return null;
        }

        public static long? ParseLong(string value)
        {
            long temp;
            if (!string.IsNullOrEmpty(value) && long.TryParse(value, out temp))
                return temp;
            return null;
        }

        public static double? ParseDouble(string value)
        {
            double temp;
            if (!string.IsNullOrEmpty(value) && double.TryParse(value, out temp))
                return temp;
            return null;
        }

        public static DateTime? ParseDateTime(string value)
        {
            DateTime temp;
            if (!string.IsNullOrEmpty(value) && DateTime.TryParseExact(value, "yyyy-MM-ddTHHmmss", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out temp))
                return temp;
            return null;
        }
    }
}