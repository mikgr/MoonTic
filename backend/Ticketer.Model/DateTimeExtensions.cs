namespace Ticketer.Model;

public static class DateTimeExtensions
{
    public static string UtcToLocalTimeString(this DateTime dateTimeOpen, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var local = TimeZoneInfo.ConvertTimeFromUtc(dateTimeOpen, tz);
        return local.ToString("MMM d yyyy HH\\:mm");
    }
}