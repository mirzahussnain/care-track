using Microsoft.AspNetCore.Routing;

namespace CareTrack.Api.Observability;

public sealed class RequestLoggingMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<RequestLoggingMiddleware> _logger;

  public RequestLoggingMiddleware(
      RequestDelegate next,
      ILogger<RequestLoggingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(
      HttpContext httpContext)
  {
    try
    {
      await _next(httpContext);
    }
    finally
    {
      var routeTemplate =
          httpContext.GetEndpoint() is RouteEndpoint endpoint
              ? endpoint.RoutePattern.RawText
              : "unmatched";

      _logger.LogInformation(
          "HTTP {Method} {RouteTemplate} responded {StatusCode}. TraceId {TraceId}",
          httpContext.Request.Method,
          routeTemplate,
          httpContext.Response.StatusCode,
          httpContext.TraceIdentifier);
    }
  }
}
