using FluentValidation;

namespace PlantBasedPizza.OrderManager.Core.CreatePickupOrder;

public class CreatePickupOrderCommandValidator : AbstractValidator<CreatePickupOrderCommand>
{
    public CreatePickupOrderCommandValidator()
    {
        RuleFor(x => x.CustomerIdentifier)
            .NotEmpty()
            .WithMessage("Customer identifier is required")
            .MaximumLength(100)
            .WithMessage("Customer identifier must be less than 100 characters")
            .Matches(@"^[a-zA-Z0-9\-_@\.]+$")
            .WithMessage("Customer identifier can only contain letters, numbers, hyphens, underscores, @ symbols, and periods");
    }
}