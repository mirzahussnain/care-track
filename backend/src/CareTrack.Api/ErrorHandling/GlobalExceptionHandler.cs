using CareTrack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

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

      _ =>
          (StatusCodes.Status500InternalServerError,
              "Internal Server Error")
    };

    if (statusCode ==
        StatusCodes.Status500InternalServerError)
    {
      _logger.LogError(
          exception,
          "Unhandled exception occurred while processing {Method} {Path}",
          httpContext.Request.Method,
          httpContext.Request.Path);
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