using FluentValidation;

namespace OrderManager.Application.Orders.Commands.CreateOrder;

/// <summary>
/// Validator for CreateOrderCommand using FluentValidation.
/// Automatically executed by ValidationBehavior in MediatR pipeline.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.PedidoId)
            .NotEmpty()
            .WithMessage("PedidoId is required.")
            .MaximumLength(50)
            .WithMessage("PedidoId must not exceed 50 characters.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ProductName)
                .NotEmpty()
                .WithMessage("ProductName is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than 0.");

            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0)
                .WithMessage("UnitPrice must be greater than 0.");
        });
    }
}
