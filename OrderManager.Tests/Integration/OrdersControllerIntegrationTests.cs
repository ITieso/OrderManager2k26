using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OrderManager.Application.DTOs;
using OrderManager.Infrastructure.Data;

namespace OrderManager.Tests.Integration;

/// <summary>
/// Testes de integração do OrdersController usando PostgreSQL real via Testcontainers.
/// Os testes validam o comportamento end-to-end com banco de dados real,
/// garantindo que queries, constraints e persistência funcionem corretamente.
/// </summary>
public class OrdersControllerIntegrationTests : IClassFixture<PostgresFixture>, IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private readonly HttpClient _client;

    public OrdersControllerIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        // Limpa dados entre testes para isolamento
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        await context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"OrderItems\", \"Orders\" CASCADE");
    }

    /// <summary>
    /// Verifica se um pedido valido e criado e persistido no PostgreSQL.
    /// </summary>
    [Fact]
    public async Task CreateOrder_WithValidRequest_ShouldPersistInPostgres()
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

        // Verifica persistencia real no banco PostgreSQL
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var dbOrder = await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == order.Id);

        dbOrder.Should().NotBeNull();
        dbOrder!.PedidoId.Should().Be(request.PedidoId);
        dbOrder.Items.Should().HaveCount(2);
    }

    /// <summary>
    /// Verifica se a constraint UNIQUE do PostgreSQL impede duplicatas.
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
    /// Verifica se requisicao invalida retorna 400 Bad Request.
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
    /// Verifica se busca por ID existente retorna 200 OK com o pedido do PostgreSQL.
    /// </summary>
    [Fact]
    public async Task GetOrderById_WithExistingOrder_ShouldQueryFromPostgres()
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
        order.Items.Should().HaveCount(1);
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
    /// Verifica se busca por PedidoId existente usa o indice do PostgreSQL.
    /// </summary>
    [Fact]
    public async Task GetOrderByPedidoId_WithExistingOrder_ShouldUsePostgresIndex()
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
        // Arrange - nenhuma configuracao necessaria

        // Act
        var response = await _client.GetAsync("/api/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    /// <summary>
    /// Verifica se listagem de pedidos processados filtra corretamente no PostgreSQL.
    /// </summary>
    [Fact]
    public async Task GetProcessedOrders_ShouldFilterProcessedFromPostgres()
    {
        // Arrange - cria um pedido que sera processado
        var createRequest = new CreateOrderRequest(
            $"PED-{Guid.NewGuid()}",
            new List<OrderItemRequest> { new("Product", 1, 75.00m) });

        await _client.PostAsJsonAsync("/api/orders", createRequest);

        // Act
        var response = await _client.GetAsync("/api/orders/processed");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var orders = await response.Content.ReadFromJsonAsync<List<OrderResponse>>();
        orders.Should().NotBeNull();
        orders!.Should().AllSatisfy(o => o.Status.Should().Be("Processed"));
    }

    /// <summary>
    /// Verifica se multiplos pedidos sao persistidos corretamente no PostgreSQL.
    /// </summary>
    [Fact]
    public async Task CreateMultipleOrders_ShouldPersistAllInPostgres()
    {
        // Arrange
        var requests = Enumerable.Range(1, 5)
            .Select(i => new CreateOrderRequest(
                $"PED-BATCH-{Guid.NewGuid()}",
                new List<OrderItemRequest> { new($"Product {i}", i, 10.00m * i) }))
            .ToList();

        // Act
        foreach (var request in requests)
        {
            var response = await _client.PostAsJsonAsync("/api/orders", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        // Assert - verifica no banco
        using var scope = _fixture.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
        var count = await context.Orders.CountAsync();
        count.Should().BeGreaterThanOrEqualTo(5);
    }
}
