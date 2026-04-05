using B2B.Commerce.Contracts.Pricing;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class CreatePriceTierRequestValidator : AbstractValidator<CreatePriceTierRequest>
{
    public CreatePriceTierRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => x.Description is not null);
    }
}
