namespace ReforaTec.Api.Common.Helpers;

public static class StringExtensions
{
    public static string ToSanitized(this string value)
    {
        var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words);
    }

    public static string ToNormalized(this string value)
    {
        return value.ToSanitized().ToLowerInvariant();
    }
}