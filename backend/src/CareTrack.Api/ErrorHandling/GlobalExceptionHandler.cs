using CareTrack.Api.Observability;
using CareTrack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CareTrack.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
  private readonly IProblemDetailsService _problemDetailsService;
  private readonly ILogger<GlobalExceptionHandler> _logger;

  public GlobalExceptionHandler(
      IProblemDetailsService problemDetailsService,
      ILogger<GlobalExceptionHandler> logger)
  {
    _problemDetailsService = problemDetailsService;
    _logger = logger;
  }

  public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken)
  {
    var (statusCode, title) = exception switch
    {
      ConflictException =>
          (StatusCodes.Status409Conflict, "Conflict"),

      ArgumentException =>
          (StatusCodes.Status400BadRequest, "Bad Request"),

      NotFoundException =>
          (StatusCodes.Status404NotFound, "Not Found"),

      ConcurrencyException =>
          (StatusCodes.Status409Conflict, "Concurrency Conflict"),

      InvalidStateTransitionException =>
          (
              StatusCodes.Status409Conflict,
              "Invalid State Transition"
          ),
      _ =>
          (StatusCodes.Status500InternalServerError,
              "Internal Server Error")
    };

    if (statusCode ==
        StatusCodes.Status500InternalServerError)
    {
      var failure =
          DatabaseFailureMetadata.From(exception);

      var routeTemplate =
          httpContext.GetEndpoint() is RouteEndpoint endpoint
              ? endpoint.RoutePattern.RawText
              : "unmatched";

      _logger.LogError(
          "Unhandled exception while processing {Method} {RouteTemplate}. Category {ExceptionCategory} SqlErrorNumber {SqlErrorNumber} RetryExhausted {RetryExhausted} TraceId {TraceId}",
          httpContext.Request.Method,
          routeTemplate,
          failure.Category,
          failure.SqlErrorNumber,
          failure.RetryExhausted,
          httpContext.TraceIdentifier);
    }

    httpContext.Response.StatusCode = statusCode;

    return await _problemDetailsService.TryWriteAsync(
        new ProblemDetailsContext
        {
          HttpContext = httpContext,
          Exception = exception,

          ProblemDetails = new ProblemDetails
          {
            Status = statusCode,
            Title = title,

            Detail = statusCode == StatusCodes.Status500InternalServerError
    ? "An unexpected error occurred."
    : exception.Message
          }
        });
  }
}