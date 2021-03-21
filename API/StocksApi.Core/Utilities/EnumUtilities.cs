using StocksApi.Core.Models;
using System;

namespace StocksApi.Core.Utilities
{
    public static class EnumUtilities
    {
        /// <summary>
        /// Returns shorthand string value for DateRange enum.
        /// </summary>
        public static string ToShortString(this DateRange dateRange)
        {
            switch (dateRange)
            {
                case DateRange.FIVEDAY: return "5d";
                case DateRange.ONEMONTH: return "1m";
                case DateRange.THREEMONTH: return "3m";
                case DateRange.SIXMONTH: return "6m";
                case DateRange.ONEYEAR: return "1y";
                default: throw new NotImplementedException($"Unimplemented string value for {nameof(DateRange)}.{dateRange}");
            }
        }
    }
}
