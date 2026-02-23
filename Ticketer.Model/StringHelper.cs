namespace Ticketer.Model;

public static class StringHelper
{
    public static string Compress(this string? str, int lengthThreshold)
    {
        if (str is null || str.Length < lengthThreshold) return str ?? "";

        int startAndEndLength =  (lengthThreshold % 2 != 0)
            ? (lengthThreshold - 1) / 2
            : lengthThreshold / 2;

        var start = str.Substring(0, startAndEndLength);
        var end = str.Substring(str.Length - startAndEndLength);

        return $"{start}...{end}";
    }
}