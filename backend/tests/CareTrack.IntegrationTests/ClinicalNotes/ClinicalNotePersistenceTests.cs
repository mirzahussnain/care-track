using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Contracts.ClinicalNotes;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.ClinicalNotes;

public sealed class ClinicalNotePersistenceTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory
      _factory;

  private readonly HttpClient
      _client;

  public ClinicalNotePersistenceTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;

    _client =
        TestAuthenticatedClient.Create(
            factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);
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
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    // Act
    var result =
        await ClinicalNoteApiTestHelper
            .CreateClinicalNoteAsync(
                _client,
                appointment.Id);

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
        TestUsers.ClinicianId,
        persisted.CreatedBy);

    Assert.Null(
        persisted.UpdatedAt);
  }
  [Fact]
  public async Task CreateClinicalNote_WhenAppointmentDoesNotExist_DoesNotPersistNote()
  {
    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{Guid.NewGuid()}/clinical-notes",
            new { content = "Clinical note" });

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);

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
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

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

    var note1 =
        await ClinicalNoteApiTestHelper
            .CreateClinicalNoteAsync(
                _client,
                appointment1.Id,
                "First note");

    var note2 =
        await ClinicalNoteApiTestHelper
            .CreateClinicalNoteAsync(
                _client,
                appointment1.Id,
                "Second note");

    await ClinicalNoteApiTestHelper
        .CreateClinicalNoteAsync(
            _client,
            appointment2.Id,
            "Different appointment note");

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
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    var created =
        await ClinicalNoteApiTestHelper
            .CreateClinicalNoteAsync(
                _client,
                appointment.Id,
                "Original content");

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
        TestUsers.ClinicianId,
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
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    await ClinicalNoteApiTestHelper
        .CreateClinicalNoteAsync(
            _client,
            appointment.Id,
            "Clinical note");

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

  [Fact]
  public async Task CreateClinicalNote_WhenRequestIsValid_ReturnsCreated()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var request =
        new
        {
          content =
                "Patient reports reduced pain."
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{appointment.Id}/clinical-notes",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<ClinicalNoteResponse>();

    Assert.NotNull(
        result);

    Assert.Equal(
        appointment.Id,
        result.AppointmentId);

    Assert.Equal(
        "Patient reports reduced pain.",
        result.Content);

    Assert.Equal(
        TestUsers.ClinicianId,
        result.CreatedBy);

    Assert.Null(
        result.UpdatedAt);
  }

  [Fact]
  public async Task CreateClinicalNote_UsesAuthenticatedUserAsCreatedBy()
  {
    var appointmentId =
        await CreateClinicalNoteAppointmentAsync();

    var result =
        await ClinicalNoteApiTestHelper
            .CreateClinicalNoteAsync(
                _client,
                appointmentId);

    Assert.Equal(
        TestUsers.ClinicianId,
        result.CreatedBy);

    using var scope =
        _factory.Services.CreateScope();

    var db =
        scope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var persisted =
        await db.ClinicalNotes
            .AsNoTracking()
            .SingleAsync(
                note => note.Id == result.Id);

    Assert.Equal(
        TestUsers.ClinicianId,
        persisted.CreatedBy);
  }

  [Fact]
  public async Task CreateClinicalNote_ClientSuppliedCreatedByCannotOverrideAuthenticatedUser()
  {
    var appointmentId =
        await CreateClinicalNoteAppointmentAsync();

    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{appointmentId}/clinical-notes",
            new
            {
              content = "Patient reports improving symptoms.",
              createdBy = "FAKE-USER"
            });

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<ClinicalNoteResponse>();

    Assert.NotNull(
        result);

    Assert.Equal(
        TestUsers.ClinicianId,
        result.CreatedBy);

    using var scope =
        _factory.Services.CreateScope();

    var db =
        scope.ServiceProvider
            .GetRequiredService<
                CareTrackDbContext>();

    var persisted =
        await db.ClinicalNotes
            .AsNoTracking()
            .SingleAsync(
                note => note.Id == result.Id);

    Assert.Equal(
        TestUsers.ClinicianId,
        persisted.CreatedBy);
  }

  [Fact]
  public async Task CreateClinicalNote_WhenUnauthenticated_ReturnsUnauthorized()
  {
    var appointmentId =
        await CreateClinicalNoteAppointmentAsync();

    using var unauthenticatedClient =
        _factory.CreateClient();

    var response =
        await unauthenticatedClient.PostAsJsonAsync(
            $"/api/appointments/{appointmentId}/clinical-notes",
            new { content = "Patient reports improving symptoms." });

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task CreateClinicalNote_WithReferralCoordinatorRole_ReturnsForbidden()
  {
    var appointmentId =
        await CreateClinicalNoteAppointmentAsync();

    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var response =
        await referralCoordinatorClient.PostAsJsonAsync(
            $"/api/appointments/{appointmentId}/clinical-notes",
            new { content = "Patient reports improving symptoms." });

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }
  [Fact]
  public async Task CreateClinicalNote_WhenAppointmentDoesNotExist_ReturnsNotFound()
  {
    var request =
        new
        {
          content =
                "Clinical note"
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{Guid.NewGuid()}/clinical-notes",
            request);

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task CreateClinicalNote_WhenContentIsBlank_ReturnsBadRequest()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var request =
        new
        {
          content = "   "
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{appointment.Id}/clinical-notes",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task CreateClinicalNote_WhenContentExceedsMaximum_ReturnsBadRequest()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var request =
        new
        {
          content =
                new string('a', 5001)
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/appointments/{appointment.Id}/clinical-notes",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }


  [Fact]
  public async Task GetClinicalNoteById_WhenNoteExists_ReturnsOk()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var note =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment.Id);

    var response =
        await _client.GetAsync(
            $"/api/clinical-notes/{note.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<ClinicalNoteResponse>();

    Assert.NotNull(
        result);

    Assert.Equal(
        note.Id,
        result.Id);

    Assert.Equal(
        appointment.Id,
        result.AppointmentId);
  }

  [Fact]
  public async Task GetClinicalNoteById_WhenNoteDoesNotExist_ReturnsNotFound()
  {
    var response =
        await _client.GetAsync(
            $"/api/clinical-notes/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task GetClinicalNotesByAppointment_ReturnsOnlyAppointmentNotes()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral1 =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var referral2 =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(5);

    var appointment1 =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var appointment2 =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral2.Id,
            scheduledStart:
                start.AddHours(1),
            scheduledEnd:
                start.AddHours(1)
                    .AddMinutes(30));

    var note1 =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment1.Id,
            "First note");

    var note2 =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment1.Id,
            "Second note");

    await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
        _client,
        appointment2.Id,
        "Different appointment");

    var response =
        await _client.GetAsync(
            $"/api/appointments/{appointment1.Id}/clinical-notes");

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var results =
        await response.Content
            .ReadFromJsonAsync<List<ClinicalNoteResponse>>();

    Assert.NotNull(
        results);

    Assert.Equal(
        2,
        results.Count);

    Assert.Contains(
        results,
        n => n.Id == note1.Id);

    Assert.Contains(
        results,
        n => n.Id == note2.Id);

    Assert.All(
        results,
        n =>
            Assert.Equal(
                appointment1.Id,
                n.AppointmentId));
  }

  [Fact]
  public async Task GetClinicalNotesByAppointment_WhenNoNotesExist_ReturnsEmptyList()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var response =
        await _client.GetAsync(
            $"/api/appointments/{appointment.Id}/clinical-notes");

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var results =
        await response.Content
            .ReadFromJsonAsync<List<ClinicalNoteResponse>>();

    Assert.NotNull(
        results);

    Assert.Empty(
        results);
  }

  [Fact]
  public async Task GetClinicalNotesByAppointment_WhenAppointmentDoesNotExist_ReturnsNotFound()
  {
    var response =
        await _client.GetAsync(
            $"/api/appointments/{Guid.NewGuid()}/clinical-notes");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task UpdateClinicalNote_WhenRequestIsValid_ReturnsUpdatedNote()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var note =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment.Id,
            "Original note");

    var request =
        new
        {
          content =
                "Updated clinical note"
        };

    var response =
        await _client.PutAsJsonAsync(
            $"/api/clinical-notes/{note.Id}",
            request);

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<ClinicalNoteResponse>();

    Assert.NotNull(
        result);

    Assert.Equal(
        "Updated clinical note",
        result.Content);

    Assert.Equal(
        TestUsers.ClinicianId,
        result.CreatedBy);

    Assert.NotNull(
        result.UpdatedAt);
  }

  [Fact]
  public async Task UpdateClinicalNote_WhenNoteDoesNotExist_ReturnsNotFound()
  {
    var request =
        new
        {
          content =
                "Updated"
        };

    var response =
        await _client.PutAsJsonAsync(
            $"/api/clinical-notes/{Guid.NewGuid()}",
            request);

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task UpdateClinicalNote_WhenContentIsBlank_ReturnsBadRequest()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var note =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment.Id);

    var request =
        new
        {
          content = "   "
        };

    var response =
        await _client.PutAsJsonAsync(
            $"/api/clinical-notes/{note.Id}",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task UpdateClinicalNote_WhenContentExceedsMaximum_ReturnsBadRequest()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var note =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment.Id);

    var request =
        new
        {
          content =
                new string('a', 5001)
        };

    var response =
        await _client.PutAsJsonAsync(
            $"/api/clinical-notes/{note.Id}",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task UpdateClinicalNote_PersistsChangeToDatabase()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _client);

    var referral =
        await ReferralApiTestHelper.CreateAssignedReferralAsync(
            _client,
            "Integration Test Team",
            passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper.CreateAppointmentAsync(
            _client,
            patient.Id,
            referral.Id);

    var note =
        await ClinicalNoteApiTestHelper.CreateClinicalNoteAsync(
            _client,
            appointment.Id);

    var request =
        new
        {
          content =
                "Persisted update"
        };

    var updateResponse =
        await _client.PutAsJsonAsync(
            $"/api/clinical-notes/{note.Id}",
            request);

    Assert.Equal(
        HttpStatusCode.OK,
        updateResponse.StatusCode);

    var getResponse =
        await _client.GetAsync(
            $"/api/clinical-notes/{note.Id}");

    var persisted =
        await getResponse.Content
            .ReadFromJsonAsync<ClinicalNoteResponse>();

    Assert.NotNull(
        persisted);

    Assert.Equal(
        "Persisted update",
        persisted.Content);

    Assert.NotNull(
        persisted.UpdatedAt);
  }


  private async Task<Guid> CreateClinicalNoteAppointmentAsync()
  {
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    return appointment.Id;
  }
}
