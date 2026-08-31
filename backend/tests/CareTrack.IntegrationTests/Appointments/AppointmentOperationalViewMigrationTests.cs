using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.Appointments;

public sealed class AppointmentOperationalViewMigrationTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public AppointmentOperationalViewMigrationTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task Migration_FromEmptyDatabase_CreatesQueryableNarrowView_AndDownDropsIt()
  {
    await using var context = CreateDisposableContext();
    await context.Database.EnsureDeletedAsync();

    try
    {
      await context.Database.MigrateAsync();
      Assert.True(await ViewExistsAsync(context));

      var patient = new Patient(
          "PAT-VIEW-001", "Amina", "Khan", new DateOnly(1988, 4, 12));
      context.Patients.Add(patient);
      await context.SaveChangesAsync();

      var referral = new Referral(
          "REF-VIEW-001", patient.Id, ReferralPriority.Routine,
          "Sensitive reason must remain outside the view.");
      context.Referrals.Add(referral);
      await context.SaveChangesAsync();

      var start = DateTime.UtcNow.AddDays(3);
      var appointment = new Appointment(
          "APT-VIEW-001", patient.Id, referral.Id,
          AppointmentType.Consultation, start, start.AddMinutes(30),
          "Operational Clinic");
      context.Appointments.Add(appointment);
      await context.SaveChangesAsync();

      var row = await context.AppointmentOperationalList.SingleAsync();
      Assert.Equal(patient.PatientReference, row.PatientReference);
      Assert.Equal(patient.FullName, row.PatientDisplayName);
      Assert.Equal(referral.ReferralReference, row.ReferralReference);

      var columns = await ViewColumnsAsync(context);
      Assert.DoesNotContain("DateOfBirth", columns);
      Assert.DoesNotContain("Reason", columns);
      Assert.DoesNotContain("TriageNote", columns);
      Assert.DoesNotContain("Content", columns);
      Assert.DoesNotContain("CreatedBy", columns);

      var applied = (await context.Database.GetAppliedMigrationsAsync()).ToArray();
      var migrator = context.Database.GetService<IMigrator>();
      await migrator.MigrateAsync(applied[^2]);

      Assert.False(await ViewExistsAsync(context));
      Assert.Equal(1, await context.Appointments.CountAsync());
    }
    finally
    {
      await context.Database.EnsureDeletedAsync();
    }
  }

  private CareTrackDbContext CreateDisposableContext()
  {
    using var scope = _factory.Services.CreateScope();
    var source = scope.ServiceProvider.GetRequiredService<CareTrackDbContext>();
    var builder = new SqlConnectionStringBuilder(
        source.Database.GetConnectionString())
    {
      InitialCatalog = $"CareTrackViewTests_{Guid.NewGuid():N}"
    };
    var options = new DbContextOptionsBuilder<CareTrackDbContext>()
        .UseSqlServer(builder.ConnectionString)
        .Options;
    return new CareTrackDbContext(options);
  }

  private static async Task<bool> ViewExistsAsync(CareTrackDbContext context)
  {
    await context.Database.OpenConnectionAsync();
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText =
        "SELECT CASE WHEN OBJECT_ID(N'[dbo].[vw_AppointmentOperationalList]', N'V') " +
        "IS NULL THEN 0 ELSE 1 END;";
    var exists = Convert.ToInt32(await command.ExecuteScalarAsync()) == 1;
    await context.Database.CloseConnectionAsync();
    return exists;
  }

  private static async Task<IReadOnlyList<string>> ViewColumnsAsync(
      CareTrackDbContext context)
  {
    await context.Database.OpenConnectionAsync();
    await using var command = context.Database.GetDbConnection().CreateCommand();
    command.CommandText =
        "SELECT [name] FROM sys.columns WHERE [object_id] = " +
        "OBJECT_ID(N'[dbo].[vw_AppointmentOperationalList]');";
    var columns = new List<string>();
    await using var reader = await command.ExecuteReaderAsync();
    while (await reader.ReadAsync())
      columns.Add(reader.GetString(0));
    await reader.DisposeAsync();
    await context.Database.CloseConnectionAsync();
    return columns;
  }
}
