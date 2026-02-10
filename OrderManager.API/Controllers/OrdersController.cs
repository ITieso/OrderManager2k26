using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using OrderManager.Application.DTOs;
using OrderManager.Application.Orders.Commands.CreateOrder;
using OrderManager.Application.Orders.Queries.GetAllOrders;
using OrderManager.Application.Orders.Queries.GetOrderById;
using OrderManager.Application.Orders.Queries.GetOrderByPedidoId;
using OrderManager.Application.Orders.Queries.GetProcessedOrders;
using OrderManager.Domain.Common;

namespace OrderManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateOrderRequest> _validator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IMediator mediator,
        IValidator<CreateOrderRequest> validator,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));
        }

        _logger.LogInformation("Creating order with PedidoId: {PedidoId}", request.PedidoId);

        var command = new CreateOrderCommand(request.PedidoId, request.Items);
        var result = await _mediator.Send(command);

        if (result.IsFailure)
        {
            return ToErrorResponse(result.Error);
        }

        _logger.LogInformation("Order created successfully. Id: {OrderId}, TaxAmount: {TaxAmount}",
            result.Value.Id, result.Value.TaxAmount);

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Value.Id }, result.Value);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id));

        if (result.IsFailure)
        {
            return ToErrorResponse(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet("pedido/{pedidoId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByPedidoId(string pedidoId)
    {
        var result = await _mediator.Send(new GetOrderByPedidoIdQuery(pedidoId));

        if (result.IsFailure)
        {
            return ToErrorResponse(result.Error);
        }

        return Ok(result.Value);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery());
        return Ok(orders);
    }

    [HttpGet("processed")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessedOrders()
    {
        _logger.LogInformation("Retrieving processed orders for System B");
        var orders = await _mediator.Send(new GetProcessedOrdersQuery());
        return Ok(orders);
    }

    private IActionResult ToErrorResponse(Error error)
    {
        return error.Code switch
        {
            var code when code.EndsWith(".NotFound") => NotFound(new { error.Code, error.Message }),
            var code when code.EndsWith(".Duplicate") => Conflict(new { error.Code, error.Message }),
            _ => BadRequest(new { error.Code, error.Message })
        };
    }
}
