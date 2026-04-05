using B2B.Commerce.Contracts.Pricing;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class SetTierPriceRequestValidator : AbstractValidator<SetTierPriceRequest>
{
    public SetTierPriceRequestValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");
    }
}
