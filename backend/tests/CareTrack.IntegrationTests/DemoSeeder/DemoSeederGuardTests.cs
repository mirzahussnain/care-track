using CareTrack.DemoSeeder;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.DemoSeeder;

public sealed class DemoSeederGuardTests(
    CareTrackSqlServerWebApplicationFactory factory)
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  [Fact]
  public async Task RunAsync_RejectsAConnectedDatabaseWhoseResolvedNameIsNotCareTrackDb()
  {
    using var scope = factory.Services.CreateScope();
    var configuration = scope.ServiceProvider
        .GetRequiredService<IConfiguration>();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();
    var connectionString = configuration.GetConnectionString(
        "IntegrationDatabase");
    var countsBefore = await ReadDomainCountsAsync(
        dbContext);
    using var output = new StringWriter();
    using var error = new StringWriter();

    var exitCode = await DemoSeederApplication.RunAsync(
        ["--target-database", "CareTrackDb"],
        () => connectionString,
        new StringReader("RESET CareTrackDb"),
        output,
        error);

    var countsAfter = await ReadDomainCountsAsync(
        dbContext);

    Assert.Equal(3, exitCode);
    Assert.Empty(output.ToString());
    Assert.Contains(
        "does not match",
        error.ToString(),
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "CareTrackIntegrationTests",
        error.ToString(),
        StringComparison.Ordinal);
    Assert.Equal(countsBefore, countsAfter);
  }

  private static async Task<int[]> ReadDomainCountsAsync(
      CareTrackDbContext dbContext)
  {
    return
    [
      await dbContext.Patients.CountAsync(),
      await dbContext.Referrals.CountAsync(),
      await dbContext.ReferralHistoryEntries.CountAsync(),
      await dbContext.Appointments.CountAsync(),
      await dbContext.ClinicalNotes.CountAsync()
    ];
  }
}
