using CareTrack.Application.ClinicalNotes.CreateClinicalNote;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class ClinicalNotesTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory
      _factory;

  private readonly HttpClient
      _client;

  public ClinicalNotesTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;

    _client =
        factory.CreateClient();
  }

  public async Task InitializeAsync()
  {
    await _factory.ResetDatabaseAsync();
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public async Task CreateClinicalNote_WithExistingAppointment_PersistsClinicalNote()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    using var scope =
        _factory.Services.CreateScope();

    var service =
        scope.ServiceProvider
            .GetRequiredService<
                CreateClinicalNoteService>();

    var command =
        new CreateClinicalNoteCommand(
            appointment.Id,
            "Patient reports improving symptoms.",
            "clinician.demo");

    // Act
    var result =
        await service.ExecuteAsync(
            command);

    // Assert
    using var verifyScope =
        _factory.Services
            .CreateScope();

    var db =
        verifyScope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var persisted =
        await db.ClinicalNotes
            .AsNoTracking()
            .SingleAsync(
                note =>
                    note.Id ==
                    result.Id);

    Assert.Equal(
        appointment.Id,
        persisted.AppointmentId);

    Assert.Equal(
        "Patient reports improving symptoms.",
        persisted.Content);

    Assert.Equal(
        "clinician.demo",
        persisted.CreatedBy);

    Assert.Null(
        persisted.UpdatedAt);
  }
  [Fact]
  public async Task CreateClinicalNote_WhenAppointmentDoesNotExist_DoesNotPersistNote()
  {
    using var scope =
        _factory.Services
            .CreateScope();

    var service =
        scope.ServiceProvider
            .GetRequiredService<
                CreateClinicalNoteService>();

    var command =
        new CreateClinicalNoteCommand(
            Guid.NewGuid(),
            "Clinical note",
            "clinician.demo");

    var action =
        () => service.ExecuteAsync(
            command);

    await Assert.ThrowsAsync<
        NotFoundException>(
        action);

    using var verifyScope =
        _factory.Services
            .CreateScope();

    var db =
        verifyScope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var count =
        await db.ClinicalNotes
            .CountAsync();

    Assert.Equal(
        0,
        count);
  }

  [Fact]
  public async Task GetByAppointmentIdAsync_ReturnsOnlyNotesForRequestedAppointment()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral1 =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient.Id);

    var start =
        DateTime.UtcNow.AddDays(3);

    var appointment1 =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    var appointment2 =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddHours(1),
                scheduledEnd:
                    start.AddHours(1)
                        .AddMinutes(30));

    using var createScope =
        _factory.Services.CreateScope();

    var service =
        createScope.ServiceProvider
            .GetRequiredService<
                CreateClinicalNoteService>();

    var note1 =
        await service.ExecuteAsync(
            new CreateClinicalNoteCommand(
                appointment1.Id,
                "First note",
                "clinician.demo"));

    var note2 =
        await service.ExecuteAsync(
            new CreateClinicalNoteCommand(
                appointment1.Id,
                "Second note",
                "clinician.demo"));

    await service.ExecuteAsync(
        new CreateClinicalNoteCommand(
            appointment2.Id,
            "Different appointment note",
            "clinician.demo"));

    // Act
    using var readScope =
        _factory.Services.CreateScope();

    var repository =
        readScope.ServiceProvider
            .GetRequiredService<
                IClinicalNoteRepository>();

    var result =
        await repository
            .GetByAppointmentIdAsync(
                appointment1.Id);

    // Assert
    Assert.Equal(
        2,
        result.Count);

    Assert.Contains(
        result,
        note =>
            note.Id ==
            note1.Id);

    Assert.Contains(
        result,
        note =>
            note.Id ==
            note2.Id);

    Assert.All(
        result,
        note =>
            Assert.Equal(
                appointment1.Id,
                note.AppointmentId));
  }

  [Fact]
  public async Task UpdateContent_AndSaveChanges_PersistsUpdatedClinicalNote()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    using var createScope =
        _factory.Services.CreateScope();

    var createService =
        createScope.ServiceProvider
            .GetRequiredService<
                CreateClinicalNoteService>();

    var created =
        await createService.ExecuteAsync(
            new CreateClinicalNoteCommand(
                appointment.Id,
                "Original content",
                "clinician.demo"));

    // Act
    using var updateScope =
        _factory.Services.CreateScope();

    var repository =
        updateScope.ServiceProvider
            .GetRequiredService<
                IClinicalNoteRepository>();

    var note =
        await repository.GetByIdAsync(
            created.Id);

    Assert.NotNull(
        note);

    note.UpdateContent(
        "Updated content");

    await repository.SaveChangesAsync();

    // Assert
    using var verifyScope =
        _factory.Services.CreateScope();

    var db =
        verifyScope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var persisted =
        await db.ClinicalNotes
            .AsNoTracking()
            .SingleAsync(
                note =>
                    note.Id ==
                    created.Id);

    Assert.Equal(
        "Updated content",
        persisted.Content);

    Assert.NotNull(
        persisted.UpdatedAt);

    Assert.Equal(
        "clinician.demo",
        persisted.CreatedBy);
  }

  [Fact]
  public async Task DeleteAppointment_WhenClinicalNoteExists_IsRestricted()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    using (var createScope =
        _factory.Services.CreateScope())
    {
      var service =
          createScope.ServiceProvider
              .GetRequiredService<
                  CreateClinicalNoteService>();

      await service.ExecuteAsync(
          new CreateClinicalNoteCommand(
              appointment.Id,
              "Clinical note",
              "clinician.demo"));
    }

    // Act
    using var deleteScope =
        _factory.Services.CreateScope();

    var db =
        deleteScope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var appointmentEntity =
        await db.Appointments
            .SingleAsync(
                a =>
                    a.Id ==
                    appointment.Id);

    db.Appointments.Remove(
        appointmentEntity);

    var action =
        () => db.SaveChangesAsync();

    // Assert
    await Assert.ThrowsAsync<
        DbUpdateException>(
        action);
  }

}