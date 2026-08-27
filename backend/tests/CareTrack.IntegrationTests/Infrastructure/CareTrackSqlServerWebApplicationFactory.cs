using CareTrack.Application.Common.Interfaces;
using CareTrack.Infrastructure.Configuration;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
      services.RemoveAll<IReferralAssignmentTargetDirectory>();
      services.AddSingleton<IReferralAssignmentTargetDirectory>(
          serviceProvider =>
          {
            var configuration = serviceProvider
                .GetRequiredService<IConfiguration>();
            var configuredTargets = configuration
                .GetSection("ReferralAssignment:Targets")
                .GetChildren()
                .Select(section => section.Value ?? string.Empty)
                .ToArray();

            return new ConfiguredReferralAssignmentTargetDirectory(
                configuredTargets);
          });

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
                    connectionString,
                    sqlServerOptions =>
                        sqlServerOptions.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay:
                                TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null));
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

    builder.ConfigureTestServices(services =>
{
  services
      .AddAuthentication(options =>
      {
        options.DefaultAuthenticateScheme =
              TestAuthenticationDefaults
                  .AuthenticationScheme;

        options.DefaultChallengeScheme =
              TestAuthenticationDefaults
                  .AuthenticationScheme;

        options.DefaultScheme =
              TestAuthenticationDefaults
                  .AuthenticationScheme;
      })
      .AddScheme<
          AuthenticationSchemeOptions,
          TestAuthenticationHandler>(
              TestAuthenticationDefaults
                  .AuthenticationScheme,
              _ =>
              {
              });
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

    dbContext.ClinicalNotes.RemoveRange(
        dbContext.ClinicalNotes);

    dbContext.Appointments.RemoveRange(
          dbContext.Appointments);

    dbContext.Referrals.RemoveRange(
        dbContext.Referrals);

    dbContext.Patients.RemoveRange(
        dbContext.Patients);

    await dbContext.SaveChangesAsync();
  }
}