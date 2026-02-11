using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OrderManager.Application.DTOs;

namespace OrderManager.Tests.Integration;

public class OrdersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrdersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// Verifica se um pedido válido é criado e retorna 201 Created.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldReturn201Created()
    {
        // Arrange
        var request = new CreateOrderRequest(
            $"PED-{Guid.NewGuid()}",
            new List<OrderItemRequest>
            {
                new("Product A", 2, 50.00m),
                new("Product B", 1, 25.00m)
            });

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order.Should().NotBeNull();
        order!.PedidoId.Should().Be(request.PedidoId);
        order.TotalAmount.Should().Be(125.00m);
        order.TaxAmount.Should().Be(37.50m);
        order.Status.Should().Be("Processed");
        order.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifica se pedido duplicado retorna 409 Conflict.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithDuplicatePedidoId_ShouldReturn409Conflict()
    {
        // Arrange
        var pedidoId = $"PED-{Guid.NewGuid()}";
        var request = new CreateOrderRequest(
            pedidoId,
            new List<OrderItemRequest> { new("Product", 1, 10.00m) });

        await _client.PostAsJsonAsync("/api/orders", request);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    /// <summary>
    /// Verifica se requisição inválida retorna 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithInvalidRequest_ShouldReturn400BadRequest()
    {
        // Arrange
        var request = new CreateOrderRequest("", new List<OrderItemRequest>());

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// Verifica se busca por ID existente retorna 200 OK com o pedido.
    /// </summary>
    [Fact]
    public async Task GetOrderById_WithExistingOrder_ShouldReturn200Ok()
    {
        // Arrange
        var createRequest = new CreateOrderRequest(
            $"PED-{Guid.NewGuid()}",
            new List<OrderItemRequest> { new("Product", 1, 100.00m) });

        var createResponse = await _client.PostAsJsonAsync("/api/orders", createRequest);
        var createdOrder = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        // Act
        var response = await _client.GetAsync($"/api/orders/{createdOrder!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.Id.Should().Be(createdOrder.Id);
    }

    /// <summary>
    /// Verifica se busca por ID inexistente retorna 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetOrderById_WithNonExistingOrder_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/orders/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifica se busca por PedidoId existente retorna 200 OK.
    /// </summary>
    [Fact]
    public async Task GetOrderByPedidoId_WithExistingOrder_ShouldReturn200Ok()
    {
        // Arrange
        var pedidoId = $"PED-{Guid.NewGuid()}";
        var createRequest = new CreateOrderRequest(
            pedidoId,
            new List<OrderItemRequest> { new("Product", 1, 50.00m) });

        await _client.PostAsJsonAsync("/api/orders", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/orders/pedido/{pedidoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        order!.PedidoId.Should().Be(pedidoId);
    }

    /// <summary>
    /// Verifica se busca por PedidoId inexistente retorna 404 Not Found.
    /// </summary>
    [Fact]
    public async Task GetOrderByPedidoId_WithNonExistingOrder_ShouldReturn404NotFound()
    {
        // Arrange
        var nonExistingPedidoId = "NON-EXISTING";

        // Act
        var response = await _client.GetAsync($"/api/orders/pedido/{nonExistingPedidoId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    /// <summary>
    /// Verifica se listagem de todos os pedidos retorna 200 OK.
    /// </summary>
    [Fact]
    public async Task GetAllOrders_ShouldReturn200Ok()
    {
        // Arrange - nenhuma configuração necessária

        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifica se listagem de pedidos processados retorna 200 OK.
    /// </summary>
    [Fact]
    public async Task GetProcessedOrders_ShouldReturn200Ok()
    {
        // Arrange - nenhuma configuração necessária

        // Act
        var response = await _client.GetAsync("/api/orders/processed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
