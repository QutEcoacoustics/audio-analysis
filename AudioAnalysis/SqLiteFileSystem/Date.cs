namespace Zio.FileSystems.Community.SqliteFileSystem
{
    using System;

    internal class Date
    {
        private static long? nowOverride;

        public static DateTime FromTicks(long ticks)
        {
            return new DateTime(ticks, DateTimeKind.Utc);
        }

        public static long ToTicks(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Unspecified:
                    throw new NotSupportedException();
                case DateTimeKind.Utc:
                    return dateTime.Ticks;
                case DateTimeKind.Local:
                    return dateTime.ToUniversalTime().Ticks;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static long Now => nowOverride ?? DateTime.UtcNow.Ticks;

        public static bool IsNowOverridden => nowOverride.HasValue;

        public static void OverrideNow(DateTime dateTime)
        {
            nowOverride = ToTicks(dateTime);
        }

        public static void ResetNow()
        {
            nowOverride = null;
        }
    }
}
