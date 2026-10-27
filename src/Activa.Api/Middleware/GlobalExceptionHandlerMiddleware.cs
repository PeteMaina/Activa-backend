using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Activa.Core.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Activa.Api.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, title, detail, type) = exception switch
        {
            DomainValidationException ex => (
                HttpStatusCode.BadRequest,
                "Domain Validation Error",
                ex.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            ),
            InvalidStatusTransitionException ex => (
                HttpStatusCode.UnprocessableEntity,
                "Invalid Status Transition",
                ex.Message,
                "https://tools.ietf.org/html/rfc4918#section-11.2"
            ),
            KeyNotFoundException ex => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                ex.Message,
                "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "An unexpected internal server error occurred",
                "A system error occurred. Please contact administrator.",
                "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Type = type,
            Instance = context.Request.Path
        };

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(problemDetails, options);

        return context.Response.WriteAsync(json);
    }
}
