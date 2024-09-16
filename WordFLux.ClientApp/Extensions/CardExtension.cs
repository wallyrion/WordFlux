using WordFLux.ClientApp.Models;
using WordFlux.Contracts;

namespace WordFLux.ClientApp.Extensions;

public static class CardExtension
{
    private static readonly TimeSpan InitialInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan MaxInterval = TimeSpan.FromDays(90); // 3 months

    public static double GetProgressRate(this CardDto card)
    {
        // Define the key intervals and corresponding progress rates
        var intervals = new (TimeSpan Interval, double Progress)[]
        {
            (TimeSpan.FromMinutes(1), 0),          // 1 minute = 0%
            (TimeSpan.FromMinutes(2), 2),          // 2 minutes = 5%
            (TimeSpan.FromMinutes(4), 4),          // 4 minutes = 7%
            (TimeSpan.FromMinutes(8), 6),         // 8 minutes = 9%
            (TimeSpan.FromMinutes(16), 8),        // 16 minutes = 11%
            (TimeSpan.FromMinutes(32), 9),        // 32 minutes = 13%
            (TimeSpan.FromMinutes(64), 10),        // 64 minutes = 15%
            (TimeSpan.FromMinutes(128), 12),       // 128 minutes = 17%
            (TimeSpan.FromMinutes(256), 14),       // 256 minutes = 19%
            (TimeSpan.FromMinutes(512), 16),       // 512 minutes = 16%
            (TimeSpan.FromMinutes(1024), 18),      // 2048 minutes = 20%
            (TimeSpan.FromDays(1), 20),            // 1 day = 20%
            (TimeSpan.FromDays(2), 30),            // 2 days = 30%
            (TimeSpan.FromDays(4), 40),            // 4 days = 40%
            (TimeSpan.FromDays(8), 50),            // 8 days = 50%
            (TimeSpan.FromDays(16), 65),           // 16 days = 65%
            (TimeSpan.FromDays(32), 80),           // 32 days = 80%
            (TimeSpan.FromDays(64), 90),           // 64 days = 90%
            (TimeSpan.FromDays(90), 95),          // 90 days = 100%
            (TimeSpan.FromDays(180), 99) ,          // 90 days = 100%
            (TimeSpan.FromDays(360), 100)           // 90 days = 100%
        };

        // If the interval is smaller than the smallest defined interval, return 0%
        if (card.ReviewInterval < intervals[0].Interval)
            return 0;

        // If the interval is greater than or equal to the largest defined interval, return 100%
        if (card.ReviewInterval >= intervals[^1].Interval)
            return 100;

        // Find the two intervals between which the ReviewInterval falls
        for (int i = 0; i < intervals.Length - 1; i++)
        {
            if (card.ReviewInterval >= intervals[i].Interval && card.ReviewInterval < intervals[i + 1].Interval)
            {
                // Linearly interpolate the progress rate between these two intervals
                var lowerInterval = intervals[i];
                var upperInterval = intervals[i + 1];

                double progressRate = lowerInterval.Progress +
                                      (upperInterval.Progress - lowerInterval.Progress) *
                                      (card.ReviewInterval.TotalMinutes - lowerInterval.Interval.TotalMinutes) /
                                      (upperInterval.Interval.TotalMinutes - lowerInterval.Interval.TotalMinutes);

                return progressRate;
            }
        }

        // In case something goes wrong, return 0 (although this should never be hit)
        return 0;
    }
}