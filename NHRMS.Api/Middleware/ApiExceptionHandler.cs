using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace NHRMS.Api.Middleware;

internal sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var sqlException = FindSqlException(exception);
        if (sqlException is not null)
        {
            if (sqlException.Number is 2601 or 2627)
            {
                httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
                await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = "A record with the same unique value already exists.",
                    Instance = httpContext.Request.Path
                }, cancellationToken);
                return true;
            }

            var schemaMismatch = sqlException.Number is 207 or 208;
            logger.LogError(exception, "Database connection failed while handling {Path}", httpContext.Request.Path);
            httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = schemaMismatch ? "Database schema out of date" : "Database unavailable",
                Detail = schemaMismatch
                    ? "The database schema is behind this API version. Apply the pending database migrations, then try again."
                    : "The database could not be reached. Check its connection and encryption settings, then try again.",
                Instance = httpContext.Request.Path
            }, cancellationToken);
            return true;
        }

        var (statusCode, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflict"),
            _ => (0, string.Empty)
        };

        if (statusCode == 0)
            return false;

        logger.LogWarning(exception, "Handled API request error with status {StatusCode}", statusCode);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = title,
            Instance = httpContext.Request.Path
        }, cancellationToken);
        return true;
    }

    private static SqlException? FindSqlException(Exception exception)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlException)
                return sqlException;
        }

        return null;
    }
}
