using System.Net;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Appointments;

public sealed class AppointmentSchedulingTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory
      _factory;

  private readonly HttpClient
      _client;

  public AppointmentSchedulingTests(
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
  public async Task
      CreateAppointment_WhenTimeExactlyMatchesExisting_ReturnsConflict()
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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    var end =
        start.AddMinutes(30);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd: end);

    // Act
    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart: start,
                scheduledEnd: end);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenRequestedStartsInsideExisting_ReturnsConflict()
  {
    // Existing:
    // 10:00 - 10:30
    //
    // Requested:
    // 10:15 - 10:45

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(15),
                scheduledEnd:
                    start.AddMinutes(45));

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenRequestedEndsInsideExisting_ReturnsConflict()
  {
    // Existing:
    // 10:00 - 10:30
    //
    // Requested:
    // 09:45 - 10:15

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(-15),
                scheduledEnd:
                    start.AddMinutes(15));

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenRequestedContainsExisting_ReturnsConflict()
  {
    // Existing:
    // 10:00 - 10:30
    //
    // Requested:
    // 09:45 - 10:45

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(-15),
                scheduledEnd:
                    start.AddMinutes(45));

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenRequestedIsInsideExisting_ReturnsConflict()
  {
    // Existing:
    // 10:00 - 11:00
    //
    // Requested:
    // 10:15 - 10:30

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddHours(1));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(15),
                scheduledEnd:
                    start.AddMinutes(30));

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenStartsExactlyAtExistingEnd_ReturnsCreated()
  {
    // Existing:
    // 10:00 - 10:30
    //
    // Requested:
    // 10:30 - 11:00

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(30),
                scheduledEnd:
                    start.AddMinutes(60));

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenEndsExactlyAtExistingStart_ReturnsCreated()
  {
    // Requested:
    // 09:30 - 10:00
    //
    // Existing:
    // 10:00 - 10:30

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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddMinutes(-30),
                scheduledEnd:
                    start);

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenTimesAreSeparate_ReturnsCreated()
  {
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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddHours(1),
                scheduledEnd:
                    start.AddHours(1)
                        .AddMinutes(30));

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenDifferentPatientHasSameTime_ReturnsCreated()
  {
    // Arrange
    var patient1 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var patient2 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral1 =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient1.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patient2.Id);

    var start =
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            _client,
            patient1.Id,
            referral1.Id,
            scheduledStart: start,
            scheduledEnd:
                start.AddMinutes(30));

    // Act
    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient2.Id,
                referral2.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenExistingAppointmentIsCancelled_ReturnsCreated()
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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    var cancelResponse =
        await _client.PostAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        cancelResponse.StatusCode);

    // Act
    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenExistingAppointmentIsDidNotAttend_ReturnsCreated()
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
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(10);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    var dnaResponse =
        await _client.PostAsync(
            $"/api/appointments/{appointment.Id}/did-not-attend",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        dnaResponse.StatusCode);

    // Act
    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                _client,
                patient.Id,
                referral2.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }
}