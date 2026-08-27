using System.Diagnostics;
using CareTrack.Api.Observability;
using CareTrack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace CareTrack.Api.Health;

public sealed class CareTrackDatabaseHealthCheck
    : IHealthCheck
{
  private readonly CareTrackDbContext _dbContext;
  private readonly ILogger<CareTrackDatabaseHealthCheck> _logger;

  public CareTrackDatabaseHealthCheck(
      CareTrackDbContext dbContext,
      ILogger<CareTrackDatabaseHealthCheck> logger)
  {
    _dbContext = dbContext;
    _logger = logger;
  }

  public async Task<HealthCheckResult> CheckHealthAsync(
      HealthCheckContext context,
      CancellationToken cancellationToken = default)
  {
    try
    {
      var canConnect =
          await _dbContext.Database
              .CanConnectAsync(cancellationToken);

      if (canConnect)
      {
        return HealthCheckResult.Healthy();
      }

      _logger.LogWarning(
          "Database readiness check reported {DatabaseFailureCategory}. TraceId {TraceId}",
          "DatabaseUnavailable",
          Activity.Current?.TraceId.ToString());

      return HealthCheckResult.Unhealthy();
    }
    catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception exception)
    {
      var failure = DatabaseFailureMetadata.From(exception);

      _logger.LogWarning(
          "Database readiness check failed. Category {DatabaseFailureCategory} SqlErrorNumber {SqlErrorNumber} RetryExhausted {RetryExhausted} TraceId {TraceId}",
          failure.Category,
          failure.SqlErrorNumber,
          failure.RetryExhausted,
          Activity.Current?.TraceId.ToString());

      return HealthCheckResult.Unhealthy();
    }
  }
}
