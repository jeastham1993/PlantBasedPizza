using FluentValidation;

namespace PlantBasedPizza.Deliver.Core.Commands;

public class MarkOrderDeliveredRequestValidator : AbstractValidator<MarkOrderDeliveredRequest>
{
    public MarkOrderDeliveredRequestValidator()
    {
        RuleFor(x => x.OrderIdentifier)
            .NotEmpty()
            .WithMessage("Order identifier is required")
            .MaximumLength(50)
            .WithMessage("Order identifier must be less than 50 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Order identifier can only contain letters, numbers, hyphens, and underscores");
    }
}