namespace WordFLux.ClientApp.Extensions;

public static class TimeUtils
{
    public static string GetNextReviewTime(TimeSpan? value)
    {
        if (value == null)
        {
            return "";
        }
        
        var time = value.Value;

        if (time < TimeSpan.FromSeconds(1))
        {
            return "a few moments";
        }

        if (time <= TimeSpan.FromMinutes(1))
        {
            return $"{time.Seconds} second{AppendS(time.Seconds)}";
        }

        if (time <= TimeSpan.FromHours(1))
        {
            return $"{time.Minutes} minute{AppendS(time.Minutes)}";
        }

        if (time <= TimeSpan.FromDays(1))
        {
            return $"{time.Hours} hour{AppendS(time.Hours)}";
        }

        if (time <= TimeSpan.FromDays(30))
        {
            return $"{time.Days} day{AppendS(time.Days)}";
        }

        if (time <= TimeSpan.FromDays(365))
        {
            var months = time.Days / 30;

            return $"{months} month{AppendS(months)}";
        }

        var years = time.Days / 365;

        return $"{years} year{AppendS(years)}";

        static string AppendS(int quantity)
        {
            return quantity > 1 ? "s" : "";
        }
    }

}