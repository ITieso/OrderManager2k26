namespace OrderManager.Application.DTOs;

/// <summary>
/// Standard API error response following RFC 7807 Problem Details.
/// </summary>
/// <param name="Type">A URI reference that identifies the problem type.</param>
/// <param name="Title">A short, human-readable summary of the problem type.</param>
/// <param name="Status">The HTTP status code.</param>
/// <param name="Detail">A human-readable explanation specific to this occurrence.</param>
/// <param name="Instance">A URI reference that identifies the specific occurrence.</param>
/// <param name="Errors">Additional validation errors, if any.</param>
public record ApiErrorResponse(
    string Type,
    string Title,
    int Status,
    string? Detail = null,
    string? Instance = null,
    IReadOnlyDictionary<string, string[]>? Errors = null
);
