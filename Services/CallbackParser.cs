using System.Text.RegularExpressions;

namespace tg_bot_timer.Services;

public static class DurationParser
{
    public static TimeSpan? Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;
        input = input.Trim().ToLowerInvariant();
        //todo add localized languages regex 
        var regex = new Regex(@"(\d+)\s*(d|day|h|hour|m|min|minute|s|sec|second)s?", RegexOptions.IgnoreCase);
        var matches = regex.Matches(input);
        if (matches.Count == 0)
            return null;
        
        var total = TimeSpan.Zero;
        foreach (Match match in matches)
        {
            if (!int.TryParse(match.Groups[1].Value, out int value))
                continue;

            string unit = match.Groups[2].Value.ToLower();

            total += unit switch
            {
                "d" or "day" or "days" => TimeSpan.FromDays(value),
                "h" or "hour" or "hours" => TimeSpan.FromHours(value),
                "m" or "min" or "minute" or "minutes" => TimeSpan.FromMinutes(value),
                "s" or "sec" or "second" or "seconds" => TimeSpan.FromSeconds(value),
                _ => TimeSpan.Zero
            };
        }

        return total.TotalSeconds > 0 ? total : null;
    }

    public static string ToApiFormat(TimeSpan ts)
    {
        if (ts.TotalDays >= 1)
            return $"{(int)ts.TotalDays}d";
        
        if (ts.TotalHours >= 1)
            return $"{(int)ts.TotalHours}h";
        
        if (ts.TotalMinutes >= 1)
            return $"{(int)ts.TotalMinutes}m";
        
        return $"{(int)ts.TotalSeconds}s";
    }
}