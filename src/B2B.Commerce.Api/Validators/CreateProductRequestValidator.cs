using B2B.Commerce.Contracts.Products;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50).WithMessage("SKU cannot exceed 50 characters")
            .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters")
            .When(x => x.Description is not null);

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category cannot exceed 100 characters")
            .When(x => x.Category is not null);
    }
}
