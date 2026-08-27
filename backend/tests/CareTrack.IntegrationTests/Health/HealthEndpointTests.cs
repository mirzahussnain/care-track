using System.Net;
using System.Net.Http.Json;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CareTrack.IntegrationTests.Health;

public sealed class HealthEndpointTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private const string SensitiveServer = "sensitive-server.invalid";
  private const string SensitiveDatabase = "SensitiveDatabase";
  private const string SensitiveUser = "SensitiveUser";
  private const string SensitivePassword = "SensitivePassword-DoNotExpose";

  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public HealthEndpointTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task Liveness_WhenDatabaseIsUnavailable_ReturnsHealthyAnonymously()
  {
    using var unavailableFactory =
        new UnavailableDatabaseWebApplicationFactory();

    using var client = unavailableFactory.CreateClient();

    var response =
        await client.GetAsync("/api/health");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body =
        await response.Content.ReadFromJsonAsync<HealthResponse>();

    Assert.NotNull(body);
    Assert.Equal("healthy", body.Status);
    Assert.Equal("CareTrack.Api", body.Service);
    Assert.Null(body.Checks);
  }

  [Fact]
  public async Task Readiness_WhenDatabaseIsAvailable_ReturnsHealthyAnonymously()
  {
    using var client = _factory.CreateClient();

    var response =
        await client.GetAsync("/api/health/ready");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var body =
        await response.Content.ReadFromJsonAsync<HealthResponse>();

    Assert.NotNull(body);
    Assert.Equal("healthy", body.Status);
    Assert.Equal("CareTrack.Api", body.Service);
    Assert.Equal("healthy", body.Checks?["database"]);
  }

  [Fact]
  public async Task Readiness_WhenDatabaseIsUnavailable_ReturnsSanitizedFailureAnonymously()
  {
    using var unavailableFactory =
        new UnavailableDatabaseWebApplicationFactory();

    using var client = unavailableFactory.CreateClient();

    var response =
        await client.GetAsync("/api/health/ready");

    Assert.Equal(
        HttpStatusCode.ServiceUnavailable,
        response.StatusCode);

    var bodyText =
        await response.Content.ReadAsStringAsync();

    var body =
        await response.Content.ReadFromJsonAsync<HealthResponse>();

    Assert.NotNull(body);
    Assert.Equal("unhealthy", body.Status);
    Assert.Equal("CareTrack.Api", body.Service);
    Assert.Equal("unhealthy", body.Checks?["database"]);

    Assert.DoesNotContain(SensitiveServer, bodyText);
    Assert.DoesNotContain(SensitiveDatabase, bodyText);
    Assert.DoesNotContain(SensitiveUser, bodyText);
    Assert.DoesNotContain(SensitivePassword, bodyText);
    Assert.DoesNotContain("SqlException", bodyText);
    Assert.DoesNotContain("connection string", bodyText, StringComparison.OrdinalIgnoreCase);
    Assert.DoesNotContain("stack", bodyText, StringComparison.OrdinalIgnoreCase);
  }

  private sealed record HealthResponse(
      string Status,
      string Service,
      Dictionary<string, string>? Checks);

  private sealed class UnavailableDatabaseWebApplicationFactory
      : WebApplicationFactory<Program>
  {
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
      builder.ConfigureServices(services =>
      {
        services.RemoveAll<
            IDbContextOptionsConfiguration<CareTrackDbContext>>();

        services.AddDbContext<CareTrackDbContext>(options =>
        {
          options.UseSqlServer(
              $"Server={SensitiveServer};" +
              $"Database={SensitiveDatabase};" +
              $"User Id={SensitiveUser};" +
              $"Password={SensitivePassword};" +
              "Connect Timeout=1;Encrypt=True;TrustServerCertificate=True;ConnectRetryCount=0");
        });
      });
    }
  }
}
