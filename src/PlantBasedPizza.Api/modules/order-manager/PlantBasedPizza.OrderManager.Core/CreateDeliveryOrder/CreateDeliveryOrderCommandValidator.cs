using FluentValidation;

namespace PlantBasedPizza.OrderManager.Core.CreateDeliveryOrder;

public class CreateDeliveryOrderCommandValidator : AbstractValidator<CreateDeliveryOrderCommand>
{
    public CreateDeliveryOrderCommandValidator()
    {
        RuleFor(x => x.CustomerIdentifier)
            .NotEmpty()
            .WithMessage("Customer identifier is required")
            .MaximumLength(100)
            .WithMessage("Customer identifier must be less than 100 characters")
            .Matches(@"^[a-zA-Z0-9\-_@\.]+$")
            .WithMessage("Customer identifier can only contain letters, numbers, hyphens, underscores, @ symbols, and periods");

        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithMessage("Address line 1 is required")
            .MaximumLength(200)
            .WithMessage("Address line 1 must be less than 200 characters");

        RuleFor(x => x.AddressLine2)
            .MaximumLength(200)
            .WithMessage("Address line 2 must be less than 200 characters");

        RuleFor(x => x.AddressLine3)
            .MaximumLength(200)
            .WithMessage("Address line 3 must be less than 200 characters");

        RuleFor(x => x.AddressLine4)
            .MaximumLength(200)
            .WithMessage("Address line 4 must be less than 200 characters");

        RuleFor(x => x.AddressLine5)
            .MaximumLength(200)
            .WithMessage("Address line 5 must be less than 200 characters");

        RuleFor(x => x.Postcode)
            .NotEmpty()
            .WithMessage("Postcode is required")
            .MaximumLength(20)
            .WithMessage("Postcode must be less than 20 characters")
            .Matches(@"^[A-Z]{1,2}\d[A-Z\d]? ?\d[A-Z]{2}$")
            .WithMessage("Postcode must be a valid UK postcode format");
    }
}