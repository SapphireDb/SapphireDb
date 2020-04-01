using System;

namespace SapphireDb.Helper
{
    public static class DateTimeHelper
    {
        public static DateTime Round(this DateTime input, TimeSpan units)
        {
            return new DateTime((input.Ticks + units.Ticks - 1) / units.Ticks * units.Ticks, input.Kind);
        }

        public static DateTime RoundToMilliseconds(this DateTime input, long accuracy = 1)
        {
            return input.Round(TimeSpan.FromMilliseconds(accuracy));
        }

        public static bool EqualWithTolerance(this DateTime input1, DateTime input2)
        {
            long input1Ticks = input1.Ticks;
            long input2Ticks = input2.Ticks;
            return input1Ticks / 10 == input2Ticks / 10;
        }
    }
}