using FluentValidation;

namespace PlantBasedPizza.OrderManager.Core.AddItemToOrder;

public class AddItemToOrderCommandValidator : AbstractValidator<AddItemToOrderCommand>
{
    public AddItemToOrderCommandValidator()
    {
        RuleFor(x => x.OrderIdentifier)
            .NotEmpty()
            .WithMessage("Order identifier is required")
            .MaximumLength(50)
            .WithMessage("Order identifier must be less than 50 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Order identifier can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.RecipeIdentifier)
            .NotEmpty()
            .WithMessage("Recipe identifier is required")
            .MaximumLength(50)
            .WithMessage("Recipe identifier must be less than 50 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Recipe identifier can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0")
            .LessThanOrEqualTo(10)
            .WithMessage("Quantity cannot exceed 10 items per single addition");
    }
}