namespace hoslog.customer.common.Utilities;

public static class TimeZoneOffsetHelper
{
    public static bool TryNormalize(
        string? timeZoneOffsetValue,
        out string normalizedValue)
    {
        normalizedValue = string.Empty;

        if (string.IsNullOrWhiteSpace(timeZoneOffsetValue))
        {
            return false;
        }

        var value = timeZoneOffsetValue.Trim();

        /*
         * Required format:
         *
         * +05:45
         * +09:00
         * -04:00
         */
        if (value.Length != 6)
        {
            return false;
        }

        if (value[0] != '+' && value[0] != '-')
        {
            return false;
        }

        if (value[3] != ':')
        {
            return false;
        }

        if (!int.TryParse(value.Substring(1, 2), out var hours))
        {
            return false;
        }

        if (!int.TryParse(value.Substring(4, 2), out var minutes))
        {
            return false;
        }

        if (hours > 14)
        {
            return false;
        }

        if (minutes > 59)
        {
            return false;
        }

        // Maximum valid UTC offset is ±14:00.
        if (hours == 14 && minutes != 0)
        {
            return false;
        }

        normalizedValue =
            $"{value[0]}{hours:00}:{minutes:00}";

        return true;
    }
}