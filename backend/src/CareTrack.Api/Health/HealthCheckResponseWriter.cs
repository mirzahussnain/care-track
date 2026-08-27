using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CareTrack.Api.Health;

public static class HealthCheckResponseWriter
{
  public static Task WriteAsync(
      HttpContext httpContext,
      HealthReport report)
  {
    httpContext.Response.ContentType =
        "application/json; charset=utf-8";

    var response = new
    {
      status = ToContractStatus(report.Status),
      service = "CareTrack.Api",
      checks = report.Entries.ToDictionary(
          entry => entry.Key,
          entry => ToContractStatus(entry.Value.Status))
    };

    return JsonSerializer.SerializeAsync(
        httpContext.Response.Body,
        response,
        cancellationToken:
            httpContext.RequestAborted);
  }

  private static string ToContractStatus(
      HealthStatus status)
  {
    return status == HealthStatus.Healthy
        ? "healthy"
        : "unhealthy";
  }
}
