using CareTrack.DemoSeeder;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.DemoSeeder;

public sealed class DemoDatabaseResetterTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private static readonly DateTime Anchor =
      new(2026, 8, 27, 18, 30, 0, DateTimeKind.Utc);
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public DemoDatabaseResetterTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  public Task InitializeAsync()
  {
    return _factory.ResetDatabaseAsync();
  }

  public Task DisposeAsync()
  {
    return _factory.ResetDatabaseAsync();
  }

  [Fact]
  public async Task ResetAsync_TwiceRestoresTheExactBaselineWithoutDuplicates()
  {
    var migrationsBefore = await GetAppliedMigrationsAsync();
    var firstCounts = await ResetAsync(
        DemoDatasetFactory.Create(Anchor));

    await AddUserGeneratedPatientAsync();

    var secondCounts = await ResetAsync(
        DemoDatasetFactory.Create(Anchor));
    var migrationsAfter = await GetAppliedMigrationsAsync();

    var expected = new DemoSeedCounts(
        12,
        17,
        94,
        10,
        7);

    Assert.Equal(expected, firstCounts);
    Assert.Equal(expected, secondCounts);
    Assert.Equal(migrationsBefore, migrationsAfter);

    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    Assert.Equal(
        12,
        await dbContext.Patients
            .Select(patient => patient.PatientReference)
            .Distinct()
            .CountAsync());
    Assert.False(await dbContext.Patients.AnyAsync(patient =>
        patient.PatientReference == "USER-GENERATED-001"));
    Assert.Equal(
        17,
        await dbContext.Referrals
            .Select(referral => referral.ReferralReference)
            .Distinct()
            .CountAsync());
    Assert.Equal(
        10,
        await dbContext.Appointments
            .Select(appointment => appointment.AppointmentReference)
            .Distinct()
            .CountAsync());
  }

  [Fact]
  public async Task ResetAsync_PersistsValidRelationshipsStatusesAndNonOverlappingWork()
  {
    await ResetAsync(
        DemoDatasetFactory.Create(Anchor));

    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    var patients = await dbContext.Patients
        .AsNoTracking()
        .ToListAsync();
    var referrals = await dbContext.Referrals
        .AsNoTracking()
        .ToListAsync();
    var history = await dbContext.ReferralHistoryEntries
        .AsNoTracking()
        .ToListAsync();
    var appointments = await dbContext.Appointments
        .AsNoTracking()
        .ToListAsync();
    var notes = await dbContext.ClinicalNotes
        .AsNoTracking()
        .ToListAsync();

    var patientIds = patients.Select(patient => patient.Id).ToHashSet();
    var referralById = referrals.ToDictionary(referral => referral.Id);
    var appointmentIds = appointments.Select(appointment => appointment.Id).ToHashSet();

    Assert.All(referrals, referral =>
        Assert.Contains(referral.PatientId, patientIds));
    Assert.All(history, entry =>
        Assert.Contains(entry.ReferralId, referralById.Keys));
    Assert.All(
        appointments,
        appointment =>
        {
          Assert.Contains(appointment.PatientId, patientIds);
          Assert.True(referralById.TryGetValue(
              appointment.ReferralId,
              out var referral));
          Assert.NotNull(referral);
          Assert.Equal(referral.PatientId, appointment.PatientId);
          Assert.True(appointment.ScheduledEnd > appointment.ScheduledStart);
        });
    Assert.All(
        notes,
        note =>
        {
          Assert.Contains(note.AppointmentId, appointmentIds);
          Assert.Equal(
              DemoDatasetFactory.ClinicianObjectId,
              note.CreatedBy);
        });

    AssertAppointmentTimestamps(appointments);
    AssertNoActiveOverlaps(appointments);
  }

  [Fact]
  public async Task ResetAsync_RollsBackDeletedRecordsWhenTheNewDatasetIsInvalid()
  {
    await ResetAsync(
        DemoDatasetFactory.Create(Anchor));

    var valid = DemoDatasetFactory.Create(Anchor);
    var duplicate = new Patient(
        valid.Patients[0].PatientReference,
        "Synthetic",
        "Duplicate",
        new DateOnly(1990, 1, 1));
    var invalid = new DemoSeedDataset(
        [.. valid.Patients, duplicate],
        valid.Referrals,
        valid.Appointments,
        valid.ClinicalNotes);

    await Assert.ThrowsAsync<DbUpdateException>(() =>
        ResetAsync(invalid));

    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    Assert.Equal(12, await dbContext.Patients.CountAsync());
    Assert.Equal(17, await dbContext.Referrals.CountAsync());
    Assert.Equal(94, await dbContext.ReferralHistoryEntries.CountAsync());
    Assert.Equal(10, await dbContext.Appointments.CountAsync());
    Assert.Equal(7, await dbContext.ClinicalNotes.CountAsync());
  }

  [Fact]
  public async Task ResetAsync_DoesNotTouchNonDomainTables()
  {
    using var setupScope = _factory.Services.CreateScope();
    var setupContext = setupScope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    await setupContext.Database.ExecuteSqlRawAsync(
        "DROP TABLE IF EXISTS [DemoSeederSentinel]");
    await setupContext.Database.ExecuteSqlRawAsync(
        "CREATE TABLE [DemoSeederSentinel] ([Id] int NOT NULL PRIMARY KEY, [Value] nvarchar(40) NOT NULL)");
    await setupContext.Database.ExecuteSqlRawAsync(
        "INSERT INTO [DemoSeederSentinel] ([Id], [Value]) VALUES (1, N'preserve-me')");

    try
    {
      await ResetAsync(
          DemoDatasetFactory.Create(Anchor));

      using var verifyScope = _factory.Services.CreateScope();
      var verifyContext = verifyScope.ServiceProvider
          .GetRequiredService<CareTrackDbContext>();
      var sentinelCount = await verifyContext.Database
          .SqlQueryRaw<int>(
              "SELECT COUNT(*) AS [Value] FROM [DemoSeederSentinel] WHERE [Value] = N'preserve-me'")
          .SingleAsync();

      Assert.Equal(1, sentinelCount);
    }
    finally
    {
      using var cleanupScope = _factory.Services.CreateScope();
      var cleanupContext = cleanupScope.ServiceProvider
          .GetRequiredService<CareTrackDbContext>();
      await cleanupContext.Database.ExecuteSqlRawAsync(
          "DROP TABLE IF EXISTS [DemoSeederSentinel]");
    }
  }

  private async Task<DemoSeedCounts> ResetAsync(
      DemoSeedDataset dataset)
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();
    var resetter = new DemoDatabaseResetter(
        dbContext);

    return await resetter.ResetAsync(dataset);
  }

  private async Task AddUserGeneratedPatientAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    dbContext.Patients.Add(
        new Patient(
            "USER-GENERATED-001",
            "Temporary",
            "Demo",
            new DateOnly(1988, 5, 4)));
    await dbContext.SaveChangesAsync();
  }

  private async Task<string[]> GetAppliedMigrationsAsync()
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext = scope.ServiceProvider
        .GetRequiredService<CareTrackDbContext>();

    return (await dbContext.Database.GetAppliedMigrationsAsync())
        .ToArray();
  }

  private static void AssertAppointmentTimestamps(
      IReadOnlyCollection<Appointment> appointments)
  {
    Assert.All(
        appointments,
        appointment =>
        {
          switch (appointment.Status)
          {
            case AppointmentStatus.Scheduled:
              Assert.Null(appointment.UpdatedAt);
              break;
            case AppointmentStatus.CheckedIn:
              Assert.NotNull(appointment.CheckedInAt);
              Assert.Null(appointment.StartedAt);
              break;
            case AppointmentStatus.InProgress:
              Assert.NotNull(appointment.CheckedInAt);
              Assert.NotNull(appointment.StartedAt);
              Assert.Null(appointment.CompletedAt);
              break;
            case AppointmentStatus.Completed:
              Assert.NotNull(appointment.CheckedInAt);
              Assert.NotNull(appointment.StartedAt);
              Assert.NotNull(appointment.CompletedAt);
              break;
            case AppointmentStatus.Cancelled:
              Assert.NotNull(appointment.CancelledAt);
              break;
            case AppointmentStatus.DidNotAttend:
              Assert.NotNull(appointment.DidNotAttendAt);
              break;
          }
        });
  }

  private static void AssertNoActiveOverlaps(
      IReadOnlyCollection<Appointment> appointments)
  {
    foreach (var patientAppointments in appointments
        .Where(appointment => appointment.Status is not AppointmentStatus.Cancelled
            and not AppointmentStatus.DidNotAttend)
        .GroupBy(appointment => appointment.PatientId))
    {
      var ordered = patientAppointments
          .OrderBy(appointment => appointment.ScheduledStart)
          .ToArray();

      for (var index = 1; index < ordered.Length; index++)
      {
        Assert.True(
            ordered[index - 1].ScheduledEnd <= ordered[index].ScheduledStart);
      }
    }
  }
}
