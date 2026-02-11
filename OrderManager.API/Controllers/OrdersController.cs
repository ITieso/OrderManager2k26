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

/// <summary>
/// Controller for managing orders.
/// Acts as middleware between System A (order source) and System B (order consumer).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(IMediator mediator, ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new order (System A endpoint).
    /// </summary>
    /// <param name="request">The order creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created order.</returns>
    /// <response code="201">Order created successfully.</response>
    /// <response code="400">Validation failed.</response>
    /// <response code="409">Order with same PedidoId already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order with PedidoId: {PedidoId}", request.PedidoId);

        var command = new CreateOrderCommand(request.PedidoId, request.Items);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            _logger.LogWarning("Order creation failed: {Error}", result.Error.Message);
            return ToProblemDetails(result.Error);
        }

        _logger.LogInformation("Order created successfully. Id: {OrderId}, TaxAmount: {TaxAmount}",
            result.Value.Id, result.Value.TaxAmount);

        return CreatedAtAction(nameof(GetOrderById), new { id = result.Value.Id }, result.Value);
    }

    /// <summary>
    /// Gets an order by its internal ID.
    /// </summary>
    /// <param name="id">The internal order ID (GUID).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">Order found.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblemDetails(result.Error);
    }

    /// <summary>
    /// Gets an order by its external PedidoId (System A identifier).
    /// </summary>
    /// <param name="pedidoId">The external order ID from System A.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The order details.</returns>
    /// <response code="200">Order found.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("pedido/{pedidoId}")]
    [ProducesResponseType(typeof(OrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderByPedidoId(
        string pedidoId,
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(new GetOrderByPedidoIdQuery(pedidoId), cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : ToProblemDetails(result.Error);
    }

    /// <summary>
    /// Gets all orders in the system.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of all orders.</returns>
    /// <response code="200">Orders retrieved successfully.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders(CancellationToken cancellationToken = default)
    {
        var orders = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Gets all processed orders (System B endpoint).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of processed orders ready for System B consumption.</returns>
    /// <response code="200">Processed orders retrieved successfully.</response>
    [HttpGet("processed")]
    [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProcessedOrders(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrieving processed orders for System B");
        var orders = await _mediator.Send(new GetProcessedOrdersQuery(), cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Converts a domain error to RFC 7807 Problem Details response.
    /// </summary>
    private IActionResult ToProblemDetails(Error error)
    {
        var statusCode = error.Code switch
        {
            var code when code.EndsWith(".NotFound") => StatusCodes.Status404NotFound,
            var code when code.EndsWith(".Duplicate") => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitleForStatusCode(statusCode),
            Status = statusCode,
            Detail = error.Message,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["code"] = error.Code;

        return StatusCode(statusCode, problemDetails);
    }

    private static string GetTitleForStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "An error occurred"
    };
}
