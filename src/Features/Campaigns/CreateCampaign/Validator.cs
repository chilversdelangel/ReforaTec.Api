using FluentValidation;

namespace ReforaTec.Api.Features.Campaigns.CreateCampaign;

public class Validator : AbstractValidator<Request>
{
    public Validator()
    {
        RuleFor(request => request.CampaignName)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);


        RuleFor(request => request.Location)
            .NotNull()
            .DependentRules(() =>
            {
                RuleFor(request => request.Location.Neighborhood)
                    .NotNull()
                    .NotEmpty();

                RuleFor(request => request.Location.Street)
                    .NotNull()
                    .NotEmpty();

                RuleFor(request => request.Location.StreetNumber)
                    .NotNull()
                    .NotEmpty();
            });

        RuleFor(request => request.Period)
            .NotNull();
    }
}