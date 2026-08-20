using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Appointments;

public class AppointmentWorkflowTests :
    IClassFixture<
        CareTrackSqlServerWebApplicationFactory>,
    IAsyncLifetime
{
  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public AppointmentWorkflowTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
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
    AppointmentWorkflow_ScheduledToCompleted_ReturnsExpectedStates()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Check in
    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var checkedIn =
        await checkInResponse.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(checkedIn);

    Assert.Equal(
        AppointmentStatus.CheckedIn,
        checkedIn.Status);

    Assert.NotNull(
        checkedIn.CheckedInAt);

    // Start
    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var inProgress =
        await startResponse.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(inProgress);

    Assert.Equal(
        AppointmentStatus.InProgress,
        inProgress.Status);

    Assert.NotNull(
        inProgress.StartedAt);

    // Complete
    var completeResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/complete",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        completeResponse.StatusCode);

    var completed =
        await completeResponse.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(completed);

    Assert.Equal(
        AppointmentStatus.Completed,
        completed.Status);

    Assert.NotNull(
        completed.CompletedAt);
  }

  [Fact]
  public async Task
    CompleteAppointment_WhenScheduled_ReturnsConflict()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/complete",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CancelAppointment_WhenScheduled_ReturnsCancelled()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        AppointmentStatus.Cancelled,
        result.Status);

    Assert.NotNull(
        result.CancelledAt);
  }

  [Fact]
  public async Task
    CancelAppointment_WhenCheckedIn_ReturnsCancelled()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        AppointmentStatus.Cancelled,
        result.Status);
  }

  [Fact]
  public async Task
    CancelAppointment_WhenInProgress_ReturnsConflict()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/cancel",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
    MarkDidNotAttend_WhenScheduled_ReturnsDidNotAttend()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/did-not-attend",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        AppointmentStatus.DidNotAttend,
        result.Status);

    Assert.NotNull(
        result.DidNotAttendAt);
  }

  [Fact]
  public async Task
    MarkDidNotAttend_WhenCheckedIn_ReturnsConflict()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/did-not-attend",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CheckInAppointment_WhenAppointmentDoesNotExist_ReturnsNotFound()
  {
    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Act
    var response =
        await clinicianClient.PostAsync(
            $"/api/appointments/{Guid.NewGuid()}/check-in",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }
}