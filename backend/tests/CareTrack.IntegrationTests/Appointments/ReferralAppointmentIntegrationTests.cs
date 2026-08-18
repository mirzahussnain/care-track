using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Appointments;

public class ReferralAppointmentIntegrationTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;
  private readonly HttpClient _client;

  public ReferralAppointmentIntegrationTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }

  [Fact]
  public async Task CreateAppointment_WhenReferralIsAssigned_MovesReferralToScheduled()
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
                "Cardiology Team",
                passedPatientId: patient.Id);

    Assert.Equal(
        ReferralStatus.Assigned,
        referral.Status);

    // Act
    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    // Assert
    Assert.NotEqual(
        Guid.Empty,
        appointment.Id);

    var referralResponse =
        await _client.GetAsync(
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
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var createReferralResponse =
        await _client.PostAsJsonAsync(
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
                _client,
                patient.Id,
                referral.Id);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);

    var getReferralResponse =
        await _client.GetAsync(
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
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team",
                passedPatientId: patient.Id);

    // Act
    var appointment =
        await AppointmentApiTestHelper
            .CreateInProgressAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.InProgress,
        appointment.Status);

    var referralResponse =
        await _client.GetAsync(
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
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team",
                passedPatientId: patient.Id);

    // Act
    var appointment =
        await AppointmentApiTestHelper
            .CreateCompletedAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.Completed,
        appointment.Status);

    var referralResponse =
        await _client.GetAsync(
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
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateCompletedAppointmentAsync(
                _client,
                patient.Id,
                referral.Id);

    Assert.Equal(
        AppointmentStatus.Completed,
        appointment.Status);

    // Act
    var completeResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/complete",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.NoContent,
        completeResponse.StatusCode);

    var referralResponse =
        await _client.GetAsync(
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
    await _client.GetAsync(
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
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team",
                passedPatientId: patient.Id);

    var firstStart =
        DateTime.UtcNow.AddDays(5);

    var completedAppointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                _client,
                patient.Id,
                referral.Id,
                AppointmentType.Consultation,
                firstStart,
                firstStart.AddMinutes(30));

    var checkInResponse =
        await _client.PostAsync(
            $"/api/appointments/{completedAppointment.Id}/check-in",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        checkInResponse.StatusCode);

    var startResponse =
        await _client.PostAsync(
            $"/api/appointments/{completedAppointment.Id}/start",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startResponse.StatusCode);

    var completeAppointmentResponse =
        await _client.PostAsync(
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
                _client,
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
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/complete",
            null);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);

    var referralResponse =
        await _client.GetAsync(
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