using B2B.Commerce.Contracts.Catalogs;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class AddProductsToCatalogRequestValidator : AbstractValidator<AddProductsToCatalogRequest>
{
    public AddProductsToCatalogRequestValidator()
    {
        RuleFor(x => x.ProductIds)
            .NotEmpty().WithMessage("At least one product ID is required")
            .Must(ids => ids.Count <= 100).WithMessage("Cannot add more than 100 products at once");

        RuleForEach(x => x.ProductIds)
            .NotEmpty().WithMessage("Product ID cannot be empty");
    }
}
