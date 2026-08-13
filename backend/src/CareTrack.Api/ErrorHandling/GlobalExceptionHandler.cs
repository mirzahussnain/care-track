using CareTrack.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.ErrorHandling;

public class GlobalExceptionHandler : IExceptionHandler
{
  private readonly IProblemDetailsService _problemDetailsService;
  public GlobalExceptionHandler(
  IProblemDetailsService problemDetailsService)
  {
    _problemDetailsService = problemDetailsService;
  }
  public async ValueTask<bool> TryHandleAsync(
  HttpContext httpContext,
  Exception exception,
  CancellationToken cancellationToken
  )
  {
    var (statusCode, title) = exception switch
    {
      ConflictException => (StatusCodes.Status409Conflict, "Conflic"),
      ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request"),
      NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
      ConcurrencyException => (StatusCodes.Status409Conflict, "Concurrency Conflict"),
      _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
    };
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
        Detail = statusCode == 500 ? "An unexpected error occurred." : exception.Message
      }
    }
    );
  }
}