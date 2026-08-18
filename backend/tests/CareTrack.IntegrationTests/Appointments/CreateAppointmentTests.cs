
using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Appointments;

public class CreateAppointmentTests :
    IClassFixture<
        CareTrackSqlServerWebApplicationFactory>,
    IAsyncLifetime
{
  private readonly HttpClient _client;

  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public CreateAppointmentTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
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
  public async Task
    CreateAppointment_WithValidRequest_ReturnsCreated()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-001",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var appointment =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(
        appointment);

    Assert.Equal(
        "APT-INT-001",
        appointment.AppointmentReference);

    Assert.Equal(
        patient.Id,
        appointment.PatientId);

    Assert.Equal(
        referral.Id,
        appointment.ReferralId);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        appointment.Status);
  }

  [Fact]
  public async Task
      CreateAppointment_WithUnknownPatient_ReturnsNotFound()
  {
    // Arrange
    var unknownPatientId =
        Guid.NewGuid();

    var unknownReferralId =
        Guid.NewGuid();

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-002",

          patientId =
                unknownPatientId,

          referralId =
                unknownReferralId,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithUnknownReferral_ReturnsNotFound()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-003",

          patientId =
                patient.Id,

          referralId =
                Guid.NewGuid(),

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WhenReferralBelongsToDifferentPatient_ReturnsBadRequest()
  {
    // Arrange
    var patientOne =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client,
                "John",
                "Smith");

    var patientTwo =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client,
                "Jane",
                "Smith");

    var referral =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patientOne.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-004",

          // Deliberately Patient Two
          patientId =
                patientTwo.Id,

          // But this belongs to Patient One
          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithDuplicateReference_ReturnsConflict()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-DUP-001",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    // Act
    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        secondResponse.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WhenEndIsBeforeStart_ReturnsBadRequest()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
       await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-005",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(-30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }


}