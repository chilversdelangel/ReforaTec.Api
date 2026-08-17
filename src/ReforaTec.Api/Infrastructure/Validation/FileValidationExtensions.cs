using FluentValidation;

namespace ReforaTec.Api.Infrastructure.Validation;

internal static class FileValidationExtensions
{
    extension<T>(IRuleBuilder<T, IFormFile?> ruleBuilder)
    {
        public IRuleBuilderOptions<T, IFormFile?> NotEmptyFile()
        {
            return ruleBuilder
                .Must(file => file is null || file.Length > 0)
                .WithMessage("'{PropertyName}' must not be empty.");
        }

        public IRuleBuilderOptions<T, IFormFile?> MaxFileSize(int maxMegabytes)
        {
            var maxBytes = (long)maxMegabytes * 1024 * 1024;

            return ruleBuilder
                .Must(file => file is null || file.Length <= maxBytes)
                .WithMessage($"'{{PropertyName}}' must not exceed {maxMegabytes} MB.");
        }

        public IRuleBuilderOptions<T, IFormFile?> AllowedExtensions(string[] allowedExtensions)
        {
            var normalized = allowedExtensions.Select(e => e.ToLowerInvariant()).ToArray();

            return ruleBuilder
                .Must(file => file is null || normalized.Contains(Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage($"'{{PropertyName}}' has an invalid extension. Allowed extensions: {string.Join(", ", allowedExtensions)}.");
        }

        public IRuleBuilderOptions<T, IFormFile?> AllowedMimeTypes(string[] allowedMimeTypes)
        {
            var normalized = allowedMimeTypes.Select(m => m.ToLowerInvariant()).ToArray();

            return ruleBuilder
                .Must(file => file is null || normalized.Contains(file.ContentType.ToLowerInvariant()))
                .WithMessage("'{PropertyName}' has an invalid content type.");
        }
    }
}
