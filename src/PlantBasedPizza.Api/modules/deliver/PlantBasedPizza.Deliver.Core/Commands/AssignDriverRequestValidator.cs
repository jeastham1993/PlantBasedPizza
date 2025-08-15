using FluentValidation;

namespace PlantBasedPizza.Deliver.Core.Commands;

public class AssignDriverRequestValidator : AbstractValidator<AssignDriverRequest>
{
    public AssignDriverRequestValidator()
    {
        RuleFor(x => x.OrderIdentifier)
            .NotEmpty()
            .WithMessage("Order identifier is required")
            .MaximumLength(50)
            .WithMessage("Order identifier must be less than 50 characters")
            .Matches(@"^[a-zA-Z0-9\-_]+$")
            .WithMessage("Order identifier can only contain letters, numbers, hyphens, and underscores");

        RuleFor(x => x.DriverName)
            .NotEmpty()
            .WithMessage("Driver name is required")
            .MinimumLength(2)
            .WithMessage("Driver name must be at least 2 characters")
            .MaximumLength(100)
            .WithMessage("Driver name must be less than 100 characters")
            .Matches(@"^[a-zA-Z\s\-'\.]+$")
            .WithMessage("Driver name can only contain letters, spaces, hyphens, apostrophes, and periods");
    }
}