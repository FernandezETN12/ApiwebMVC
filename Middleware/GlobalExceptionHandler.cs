using ApiwebMVC.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ApiwebMVC.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title, errors) = exception switch
        {
            ApiwebMVC.Exceptions.ValidationException validation =>
                (StatusCodes.Status400BadRequest, validation.Message, validation.Errors),
            NotFoundException notFound =>
                (StatusCodes.Status404NotFound, notFound.Message, null),
            ConflictException conflict =>
                (StatusCodes.Status409Conflict, conflict.Message, null),
            BadHttpRequestException =>
                (StatusCodes.Status400BadRequest, "La solicitud contiene datos inválidos.", null),
            _ =>
                (StatusCodes.Status500InternalServerError, "Ocurrió un error interno en el servidor.", null)
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Error no controlado procesando {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = status == 500 ? "https://httpstatuses.com/500" : null,
            Instance = context.Request.Path
        };

        if (errors is not null)
            problem.Extensions["errors"] = errors;

        await context.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
