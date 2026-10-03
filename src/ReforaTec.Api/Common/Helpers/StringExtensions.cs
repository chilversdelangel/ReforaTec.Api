namespace ReforaTec.Api.Common.Helpers;

internal static class StringExtensions
{
    extension(string value)
    {
        public string ToSanitized()
        {
            var words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", words);
        }

        public string ToNormalized()
        {
            return value.ToSanitized().ToLowerInvariant();
        }

        public string ToNormalizedPath()
        {
            return value.Trim('/', '\\').Replace('\\', '/');
        }
    }
}