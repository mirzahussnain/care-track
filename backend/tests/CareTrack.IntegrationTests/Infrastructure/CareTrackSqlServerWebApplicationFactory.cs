using CareTrack.Infrastructure.Persistance;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.Infrastructure;

public sealed class CareTrackSqlServerWebApplicationFactory
    : WebApplicationFactory<Program>
{

  protected override void ConfigureWebHost(
      IWebHostBuilder builder)
  {
    builder.ConfigureAppConfiguration(
    (context, configuration) =>
    {
      var integrationSettingsPath = Path.Combine(
          AppContext.BaseDirectory,
          "appsettings.Integration.json");

      configuration.AddJsonFile(
          integrationSettingsPath,
          optional: false);
    });

    builder.ConfigureServices(services =>
    {
      var descriptor =
              services.SingleOrDefault(
                  service =>
                      service.ServiceType ==
                      typeof(
                          IDbContextOptionsConfiguration<
                              CareTrackDbContext>));

      if (descriptor is not null)
      {
        services.Remove(descriptor);
      }

      var configuration =
              services.BuildServiceProvider()
                  .GetRequiredService<IConfiguration>();

      var connectionString =
              configuration.GetConnectionString(
                  "IntegrationDatabase");

      if (string.IsNullOrWhiteSpace(connectionString))
      {
        throw new InvalidOperationException(
                "Integration database connection string was not found.");
      }

      services.AddDbContext<CareTrackDbContext>(
              options =>
              {
                options.UseSqlServer(
                        connectionString);
              });

      var serviceProvider =
              services.BuildServiceProvider();

      using var scope =
              serviceProvider.CreateScope();

      var dbContext =
              scope.ServiceProvider
                  .GetRequiredService<CareTrackDbContext>();

      dbContext.Database.Migrate();
    });
  }
  public async Task ResetDatabaseAsync()
  {
    using var scope =
        Services.CreateScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    dbContext.Appointments.RemoveRange(
          dbContext.Appointments);

    dbContext.ClinicalNotes.RemoveRange(
        dbContext.ClinicalNotes);

    dbContext.Referrals.RemoveRange(
        dbContext.Referrals);

    dbContext.Patients.RemoveRange(
        dbContext.Patients);

    await dbContext.SaveChangesAsync();
  }
}