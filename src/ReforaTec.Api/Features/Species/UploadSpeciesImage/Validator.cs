using FluentValidation;
using ReforaTec.Api.Infrastructure.Validation;

namespace ReforaTec.Api.Features.Species.UploadSpeciesImage;

internal sealed class Validator : AbstractValidator<Request>
{
    private static readonly string[] AllowedExtensions = [".png", ".webp", ".jpg", ".jpeg"];
    private static readonly string[] AllowedMimeTypes = ["image/png", "image/webp", "image/jpeg"];

    public Validator()
    {
        RuleFor(x => x.SpeciesImage)
            .NotNull()
            .NotEmptyFile()
            .MaxFileSize(maxMegabytes: 5)
            .AllowedExtensions(AllowedExtensions)
            .AllowedMimeTypes(AllowedMimeTypes);
    }
}
