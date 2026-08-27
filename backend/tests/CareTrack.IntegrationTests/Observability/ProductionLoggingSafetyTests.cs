using System.Text;
using CareTrack.Api.ErrorHandling;
using CareTrack.Api.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.Logging;

namespace CareTrack.IntegrationTests.Observability;

public sealed class ProductionLoggingSafetyTests
{
  private const string SensitivePatientValue = "PATIENT-SENTINEL";
  private const string SensitiveClinicalValue = "CLINICAL-NOTE-SENTINEL";
  private const string SensitiveToken = "TOKEN-SENTINEL";
  private const string SensitiveExceptionMessage =
      "EXCEPTION-SENTINEL Server=sensitive;Password=secret";

  [Fact]
  public async Task RequestLogging_DoesNotCaptureRouteValuesQueryHeadersOrBody()
  {
    var logger =
        new CapturingLogger<RequestLoggingMiddleware>();

    var middleware =
        new RequestLoggingMiddleware(
            context =>
            {
              context.Response.StatusCode =
                  StatusCodes.Status204NoContent;

              return Task.CompletedTask;
            },
            logger);

    var context = new DefaultHttpContext();
    context.TraceIdentifier = "trace-safe-001";
    context.Request.Method = HttpMethods.Put;
    context.Request.Path =
        $"/api/patients/{SensitivePatientValue}";
    context.Request.QueryString =
        new QueryString($"?note={SensitiveClinicalValue}");
    context.Request.Headers.Authorization =
        $"Bearer {SensitiveToken}";
    context.Request.Body =
        new MemoryStream(
            Encoding.UTF8.GetBytes(SensitiveClinicalValue));
    context.SetEndpoint(
        CreateRouteEndpoint("/api/patients/{id}"));

    await middleware.InvokeAsync(context);

    var log = Assert.Single(logger.Messages);

    Assert.Contains("PUT", log);
    Assert.Contains("/api/patients/{id}", log);
    Assert.Contains("204", log);
    Assert.Contains("trace-safe-001", log);
    Assert.DoesNotContain(SensitivePatientValue, log);
    Assert.DoesNotContain(SensitiveClinicalValue, log);
    Assert.DoesNotContain(SensitiveToken, log);
  }

  [Fact]
  public async Task UnexpectedExceptionLogging_DoesNotCaptureExceptionMessageOrRouteValues()
  {
    var logger =
        new CapturingLogger<GlobalExceptionHandler>();

    var handler =
        new GlobalExceptionHandler(
            new StubProblemDetailsService(),
            logger);

    var context = new DefaultHttpContext();
    context.TraceIdentifier = "trace-safe-002";
    context.Request.Method = HttpMethods.Get;
    context.Request.Path =
        $"/api/referrals/{SensitivePatientValue}";
    context.SetEndpoint(
        CreateRouteEndpoint("/api/referrals/{id}"));

    await handler.TryHandleAsync(
        context,
        new InvalidOperationException(
            SensitiveExceptionMessage),
        CancellationToken.None);

    var log = Assert.Single(logger.Messages);

    Assert.Contains("InvalidOperationException", log);
    Assert.Contains("/api/referrals/{id}", log);
    Assert.Contains("trace-safe-002", log);
    Assert.DoesNotContain(SensitivePatientValue, log);
    Assert.DoesNotContain(SensitiveExceptionMessage, log);
    Assert.DoesNotContain("Password=secret", log);
  }

  private static RouteEndpoint CreateRouteEndpoint(
      string routeTemplate)
  {
    return new RouteEndpoint(
        _ => Task.CompletedTask,
        RoutePatternFactory.Parse(routeTemplate),
        order: 0,
        EndpointMetadataCollection.Empty,
        displayName: routeTemplate);
  }

  private sealed class CapturingLogger<T>
      : ILogger<T>
  {
    public List<string> Messages { get; } = [];

    public IDisposable? BeginScope<TState>(
        TState state)
        where TState : notnull
    {
      return null;
    }

    public bool IsEnabled(
        LogLevel logLevel)
    {
      return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
      Messages.Add(formatter(state, exception));
    }
  }

  private sealed class StubProblemDetailsService
      : IProblemDetailsService
  {
    public ValueTask WriteAsync(
        ProblemDetailsContext context)
    {
      return ValueTask.CompletedTask;
    }

    public ValueTask<bool> TryWriteAsync(
        ProblemDetailsContext context)
    {
      return ValueTask.FromResult(true);
    }
  }
}
