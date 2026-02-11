namespace OrderManager.Domain.Common;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, string identifier) =>
        new($"{entity}.NotFound", $"{entity} with identifier '{identifier}' was not found.");

    public static Error Duplicate(string entity, string identifier) =>
        new($"{entity}.Duplicate", $"{entity} with identifier '{identifier}' already exists.");

    public static Error Validation(string message) =>
        new("Validation.Error", message);
}

public static class OrderErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Order", id.ToString());

    public static Error NotFoundByPedidoId(string pedidoId) =>
        Error.NotFound("Order", pedidoId);

    public static Error Duplicate(string pedidoId) =>
        Error.Duplicate("Order", pedidoId);
}
