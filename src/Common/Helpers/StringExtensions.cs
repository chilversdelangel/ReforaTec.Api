namespace ReforaTec.Api.Common.Helpers;

public static class StringExtensions
{
    public static string ToNormalized(this string value)
    {
        var trimmed = value.Trim();
        var words = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var joined = string.Join(" ", words);

        return joined.ToLowerInvariant();
    }
}