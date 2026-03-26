using B2B.Commerce.Contracts.Catalogs;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class UpdateCatalogRequestValidator : AbstractValidator<UpdateCatalogRequest>
{
    public UpdateCatalogRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => x.Description is not null);
    }
}
