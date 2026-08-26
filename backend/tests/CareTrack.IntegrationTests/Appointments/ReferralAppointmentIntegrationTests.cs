using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Appointments;

public class ReferralAppointmentIntegrationTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public ReferralAppointmentIntegrationTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task CreateAppointment_WhenReferralIsAssigned_MovesReferralToScheduled()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                ReferralTestAssignmentTargets.CardiologyTeamA,
                passedPatientId: patient.Id);

    Assert.Equal(
        ReferralStatus.Assigned,
        referral.Status);

    // Act
    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Assert
    Assert.NotEqual(
        Guid.Empty,
        appointment.Id);

    var referralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        referralResponse.StatusCode);

    var updatedReferral =
        await referralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        updatedReferral);

    Assert.Equal(
        ReferralStatus.Scheduled,
        updatedReferral!.Status);
  }

  [Fact]
  public async Task CreateAppointment_WhenReferralIsDraft_ReturnsConflict()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var createReferralResponse =
        await referralCoordinatorClient.PostAsJsonAsync(
            "/api/referrals",
            new
            {
              referralReference =
                    $"REF-{Guid.NewGuid():N}"[..16],

              patientId =
                    patient.Id,

              priority =
                    (int)ReferralPriority.Routine,

              reason =
                    "Draft referral test."
            });

    Assert.Equal(
        HttpStatusCode.Created,
        createReferralResponse.StatusCode);

    var referral =
        await createReferralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        referral);

    Assert.Equal(
        ReferralStatus.Draft,
        referral!.Status);

    // Act
    var response =
        await AppointmentApiTestHelper
            .SendCreateAppointmentRequestAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);

    var getReferralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    var unchangedReferral =
        await getReferralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        unchangedReferral);

    Assert.Equal(
        ReferralStatus.Draft,
        unchangedReferral!.Status);
  }

  [Fact]
  public async Task StartAppointment_WhenReferralIsScheduled_MovesReferralToInProgress()
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
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                ReferralTestAssignmentTargets.CardiologyTeamA,
                passedPatientId: patient.Id);

    // Act
    var scheduledAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var appointment =
        await startResponse.Content
            .ReadFromJsonAsync<AppointmentResponse>();

    Assert.NotNull(
        appointment);

    // Assert
    Assert.Equal(
        AppointmentStatus.InProgress,
        appointment.Status);

    var referralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        referralResponse.StatusCode);

    var updatedReferral =
        await referralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        updatedReferral);

    Assert.Equal(
        ReferralStatus.InProgress,
        updatedReferral!.Status);
  }

  [Fact]
  public async Task CompleteAppointment_DoesNotAutomaticallyCompleteReferral()
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
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                ReferralTestAssignmentTargets.CardiologyTeamA,
                passedPatientId: patient.Id);

    // Act
    var scheduledAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var completeAppointmentResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/complete",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        completeAppointmentResponse.StatusCode);

    var appointment =
        await completeAppointmentResponse.Content
            .ReadFromJsonAsync<AppointmentResponse>();

    Assert.NotNull(
        appointment);

    // Assert
    Assert.Equal(
        AppointmentStatus.Completed,
        appointment.Status);

    var referralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        referralResponse.StatusCode);

    var updatedReferral =
        await referralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        updatedReferral);

    Assert.Equal(
        ReferralStatus.InProgress,
        updatedReferral!.Status);
  }

  [Fact]
  public async Task CompleteReferral_WhenCompletedAppointmentExists_ReturnsNoContentAndCompletesReferral()
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
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                ReferralTestAssignmentTargets.CardiologyTeamA,
                passedPatientId: patient.Id);

    var scheduledAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var completeAppointmentResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{scheduledAppointment.Id}/complete",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        completeAppointmentResponse.StatusCode);

    var appointment =
        await completeAppointmentResponse.Content
            .ReadFromJsonAsync<AppointmentResponse>();

    Assert.NotNull(
        appointment);

    Assert.Equal(
        AppointmentStatus.Completed,
        appointment.Status);

    // Act
    var completeResponse =
        await referralCoordinatorClient.PostAsync(
            $"/api/referrals/{referral.Id}/complete",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.NoContent,
        completeResponse.StatusCode);

    var referralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    var completedReferral =
        await referralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        completedReferral);

    Assert.Equal(
        ReferralStatus.Completed,
        completedReferral!.Status);

    var historyResponse =
    await referralCoordinatorClient.GetAsync(
        $"/api/referrals/{referral.Id}/history");

    Assert.Equal(
        HttpStatusCode.OK,
        historyResponse.StatusCode);

    var history =
        await historyResponse.Content
            .ReadFromJsonAsync<
                List<ReferralHistoryResponse>>();

    Assert.NotNull(
        history);

    Assert.Contains(
        history!,
        entry =>
            entry.EventType ==
            ReferralHistoryEventType.Completed
            &&
            entry.FromStatus ==
            ReferralStatus.InProgress
            &&
            entry.ToStatus ==
            ReferralStatus.Completed);
  }

  [Fact]
  public async Task CompleteReferral_WhenScheduledAppointmentRemains_ReturnsConflict()
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
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                ReferralTestAssignmentTargets.CardiologyTeamA,
                passedPatientId: patient.Id);

    var firstStart =
        DateTime.UtcNow.AddDays(5);

    var completedAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id,
                AppointmentType.Consultation,
                firstStart,
                firstStart.AddMinutes(30));

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{completedAppointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{completedAppointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var completeAppointmentResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{completedAppointment.Id}/complete",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        completeAppointmentResponse.StatusCode);

    var secondStart =
        firstStart.AddHours(2);

    var scheduledAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id,
                AppointmentType.FollowUp,
                secondStart,
                secondStart.AddMinutes(30));

    Assert.Equal(
        AppointmentStatus.Scheduled,
        scheduledAppointment.Status);

    // Act
    var response =
        await referralCoordinatorClient.PostAsync(
            $"/api/referrals/{referral.Id}/complete",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);

    var referralResponse =
        await referralCoordinatorClient.GetAsync(
            $"/api/referrals/{referral.Id}");

    var unchangedReferral =
        await referralResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        unchangedReferral);

    Assert.Equal(
        ReferralStatus.InProgress,
        unchangedReferral!.Status);
  }
}