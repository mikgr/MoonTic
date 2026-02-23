namespace Ticketer.Web.Extensions;

public static class DateTimeExtensions
{
    public static string ToTimeAgo(this DateTimeOffset dateTimeUtc)
    {
        var timeSpan = DateTimeOffset.UtcNow - dateTimeUtc;

        return timeSpan switch
        {
            { TotalSeconds: < 60 } => $"{(int)timeSpan.TotalSeconds} secs ago",
            { TotalMinutes: < 60 } => $"{(int)timeSpan.TotalMinutes} mins ago",
            { TotalHours: < 24 } => $"{(int)timeSpan.TotalHours} hrs ago",
            { TotalDays: < 30 } => $"{(int)timeSpan.TotalDays} days ago",
            { TotalDays: < 365 } => $"{(int)(timeSpan.TotalDays / 30)} months ago",
            _ => $"{(int)(timeSpan.TotalDays / 365)} years ago"
        };
    }
}
