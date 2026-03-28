using B2B.Commerce.Contracts.Orders;
using FluentValidation;

namespace B2B.Commerce.Api.Validators;

public class CreateOrderRequestValidator : AbstractValidator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CartId)
            .NotEmpty().WithMessage("Cart ID is required");

        RuleFor(x => x.PurchaseOrderNumber)
            .MaximumLength(100).WithMessage("Purchase order number cannot exceed 100 characters")
            .When(x => x.PurchaseOrderNumber is not null);
    }
}
