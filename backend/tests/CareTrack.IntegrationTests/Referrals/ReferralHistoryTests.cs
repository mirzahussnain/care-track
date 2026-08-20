using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Referrals;

public class ReferralHistoryTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public ReferralHistoryTests(CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
    _client =
        TestAuthenticatedClient.Create(
            factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);
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
  public async Task GetHistory_AfterReferralWorkflow_ReturnsEventsInOrder()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await ReferralApiTestHelper.CreateReferralAsync(_client);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/submit",
        null);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/start-triage",
        null);

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/triage-assessment",
        new
        {
          priority =
                ReferralPriority.Urgent,

          note =
                "Urgent specialist review required."
        });

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/accept",
        null);

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/assign",
        new
        {
          assignedTo =
                "Cardiology Team A"
        });

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/reassign",
        new
        {
          assignedTo =
                "Cardiology Team B"
        });

    // Act
    var response =
        await _client.GetAsync(
            $"/api/referrals/{referral.Id}/history");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var history =
        await response.Content
            .ReadFromJsonAsync<
                List<ReferralHistoryResponse>>();

    Assert.NotNull(
        history);

    Assert.Equal(
        7,
        history.Count);

    Assert.Equal(
        ReferralHistoryEventType.Created,
        history[0].EventType);

    Assert.Equal(
        ReferralHistoryEventType.Submitted,
        history[1].EventType);

    Assert.Equal(
        ReferralHistoryEventType.TriageStarted,
        history[2].EventType);

    Assert.Equal(
        ReferralHistoryEventType.TriageAssessmentRecorded,
        history[3].EventType);

    Assert.Equal(
        ReferralHistoryEventType.Accepted,
        history[4].EventType);

    Assert.Equal(
        ReferralHistoryEventType.Assigned,
        history[5].EventType);

    Assert.Equal(
        ReferralHistoryEventType.Reassigned,
        history[6].EventType);
  }

  [Fact]
  public async Task GetHistory_AfterTriageReassessment_PreservesBothAssessments()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await ReferralApiTestHelper.CreateReferralAsync(_client);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/submit",
        null);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/start-triage",
        null);

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/triage-assessment",
        new
        {
          priority =
                ReferralPriority.Routine,

          note =
                "Routine review."
        });

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/triage-assessment",
        new
        {
          priority =
                ReferralPriority.Urgent,

          note =
                "Condition deteriorated."
        });

    // Act
    var history =
        await _client.GetFromJsonAsync<
            List<ReferralHistoryResponse>>(
                $"/api/referrals/{referral.Id}/history");

    // Assert
    Assert.NotNull(
        history);

    var assessments =
        history
            .Where(
                entry =>
                    entry.EventType ==
                    ReferralHistoryEventType
                        .TriageAssessmentRecorded)
            .ToList();

    Assert.Equal(
        2,
        assessments.Count);

    Assert.Equal(
        ReferralPriority.Routine,
        assessments[0].Priority);

    Assert.Equal(
        "Routine review.",
        assessments[0].TriageNote);

    Assert.Equal(
        ReferralPriority.Urgent,
        assessments[1].Priority);

    Assert.Equal(
        "Condition deteriorated.",
        assessments[1].TriageNote);
  }

  [Fact]
  public async Task GetHistory_AfterReassignment_PreservesPreviousAssignment()
  {
    await _factory.ResetDatabaseAsync();

    var referral =
        await ReferralApiTestHelper.CreateAcceptedReferralAsync(_client);

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/assign",
        new
        {
          assignedTo =
                "Cardiology Team A"
        });

    await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/reassign",
        new
        {
          assignedTo =
                "Cardiology Team B"
        });

    var history =
        await _client.GetFromJsonAsync<
            List<ReferralHistoryResponse>>(
                $"/api/referrals/{referral.Id}/history");

    Assert.NotNull(
        history);

    var assignments =
        history
            .Where(
                entry =>
                    entry.EventType ==
                        ReferralHistoryEventType.Assigned
                    ||
                    entry.EventType ==
                        ReferralHistoryEventType.Reassigned)
            .ToList();

    Assert.Equal(
        2,
        assignments.Count);

    Assert.Equal(
        "Cardiology Team A",
        assignments[0].AssignedTo);

    Assert.Equal(
        "Cardiology Team B",
        assignments[1].AssignedTo);
  }

  [Fact]
  public async Task GetHistory_WithUnknownReferral_ReturnsNotFound()
  {
    await _factory.ResetDatabaseAsync();

    var response =
        await _client.GetAsync(
            $"/api/referrals/{Guid.NewGuid()}/history");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task FailedAssignment_DoesNotCreateHistoryEntry()
  {
    await _factory.ResetDatabaseAsync();

    var referral =
        await ReferralApiTestHelper.CreateReferralAsync(_client);

    // Draft Referral — assignment should fail.
    var assignResponse =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo =
                    "Cardiology Team A"
            });

    Assert.Equal(
        HttpStatusCode.Conflict,
        assignResponse.StatusCode);

    var history =
        await _client.GetFromJsonAsync<
            List<ReferralHistoryResponse>>(
                $"/api/referrals/{referral.Id}/history");

    Assert.NotNull(
        history);

    Assert.Single(
        history);

    Assert.Equal(
        ReferralHistoryEventType.Created,
        history[0].EventType);
  }

}