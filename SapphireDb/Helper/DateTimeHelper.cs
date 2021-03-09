using System;

namespace SapphireDb.Helper
{
    public static class DateTimeHelper
    {
        public static DateTimeOffset Round(this DateTimeOffset input, TimeSpan units)
        {
            return new DateTimeOffset((input.Ticks + units.Ticks - 1) / units.Ticks * units.Ticks, TimeSpan.Zero);
        }

        public static bool EqualWithTolerance(this DateTimeOffset input1, DateTimeOffset input2, string databaseProviderName)
        {
            long input1Ticks = input1.Ticks;
            long input2Ticks = input2.Ticks;

            if (databaseProviderName == "Npgsql.EntityFrameworkCore.PostgreSQL")
            {
                input1Ticks /= 10;
                input2Ticks /= 10;   
            }

            return input1Ticks == input2Ticks;
        }
    }
}