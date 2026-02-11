using System.Reflection;
using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using OrderManager.Application;
using OrderManager.Infrastructure.Data;
using OrderManager.Infrastructure.ExternalServices;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog configuration
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    // Controllers with strict JSON validation
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            // Reject invalid JSON values (letters in numeric fields, etc.)
            options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            // Custom response for model binding/validation errors
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(e => e.Value?.Errors.Count > 0)
                    .ToDictionary(
                        e => e.Key,
                        e => e.Value!.Errors.Select(err =>
                            string.IsNullOrEmpty(err.ErrorMessage)
                                ? "Invalid value provided"
                                : err.ErrorMessage).ToArray()
                    );

                var problemDetails = new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Invalid Request",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "The request contains invalid data. Please check the errors.",
                    Instance = context.HttpContext.Request.Path
                };
                problemDetails.Extensions["errors"] = errors;

                return new BadRequestObjectResult(problemDetails)
                {
                    ContentTypes = { "application/problem+json" }
                };
            };
        });

    // Swagger with XML documentation
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "OrderManager API",
            Version = "v1",
            Description = "Order processing middleware between System A and System B",
            Contact = new OpenApiContact
            {
                Name = "Development Team"
            }
        });

        // Include XML comments
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

    // Application services
    builder.Services.AddApplication();
    builder.Services.AddDataInfrastructure();
    builder.Services.AddExternalServices();

    // Problem Details for consistent error responses
    builder.Services.AddProblemDetails();

    var app = builder.Build();

    // Global exception handler
    app.UseExceptionHandler(exceptionHandlerApp =>
    {
        exceptionHandlerApp.Run(async context =>
        {
            var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();
            var exception = exceptionHandlerFeature?.Error;

            var problemDetails = exception switch
            {
                ValidationException validationException => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Validation Failed",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "One or more validation errors occurred.",
                    Instance = context.Request.Path,
                    Extensions =
                    {
                        ["errors"] = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                    }
                },
                JsonException jsonException => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Invalid JSON",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = $"The request body contains invalid JSON: {jsonException.Message}",
                    Instance = context.Request.Path
                },
                _ => new ProblemDetails
                {
                    Type = "https://httpstatuses.com/500",
                    Title = "Internal Server Error",
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = app.Environment.IsDevelopment()
                        ? exception?.Message
                        : "An unexpected error occurred.",
                    Instance = context.Request.Path
                }
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

            Log.Error(exception, "Unhandled exception occurred");

            await context.Response.WriteAsJsonAsync(problemDetails);
        });
    });

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrderManager API v1");
            options.DisplayRequestDuration();
        });
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    Log.Information("Starting OrderManager API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program { }
