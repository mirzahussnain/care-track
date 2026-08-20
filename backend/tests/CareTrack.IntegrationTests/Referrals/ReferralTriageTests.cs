using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Referrals;

public class ReferralTriageTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public ReferralTriageTests(CareTrackSqlServerWebApplicationFactory factory)
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

  private async Task<ReferralResponse>
    CreateAwaitingTriageReferralAsync()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    var submitResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/submit",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        submitResponse.StatusCode);

    var triageResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/start-triage",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        triageResponse.StatusCode);

    var result =
        await triageResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(result);

    return result;
  }

  [Fact]
  public async Task RecordTriageAssessment_WhenAwaitingTriage_ReturnsUpdatedReferral()
  {
    // Arrange
    var referral =
        await CreateAwaitingTriageReferralAsync();

    var request =
        new
        {
          priority =
                ReferralPriority.Urgent,

          note =
                "Symptoms have deteriorated."
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/triage-assessment",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var updated =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralPriority.Urgent,
        updated.Priority);

    Assert.Equal(
        "Symptoms have deteriorated.",
        updated.TriageNote);

    Assert.NotNull(
        updated.TriagedAt);
  }

  [Fact]
  public async Task RecordTriageAssessment_WhenDraft_ReturnsConflict()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    var request =
        new
        {
          priority =
                ReferralPriority.Urgent,

          note =
                "Urgent review required."
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/triage-assessment",
            request);

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task RecordTriageAssessment_WithUnknownReferral_ReturnsNotFound()
  {
    var request =
        new
        {
          priority =
                ReferralPriority.Urgent,

          note =
                "Urgent review required."
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{Guid.NewGuid()}/triage-assessment",
            request);

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task RecordTriageAssessment_WithBlankNote_ReturnsBadRequest()
  {
    var referral =
        await CreateAwaitingTriageReferralAsync();

    var request =
        new
        {
          priority =
                ReferralPriority.Urgent,

          note = ""
        };

    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/triage-assessment",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task RecordTriageAssessment_WhenRepeated_UpdatesLatestAssessment()
  {
    var referral =
        await CreateAwaitingTriageReferralAsync();

    var first =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/triage-assessment",
            new
            {
              priority =
                    ReferralPriority.Routine,

              note =
                    "Routine assessment."
            });

    Assert.Equal(
        HttpStatusCode.OK,
        first.StatusCode);

    var second =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/triage-assessment",
            new
            {
              priority =
                    ReferralPriority.Urgent,

              note =
                    "Condition has deteriorated."
            });

    Assert.Equal(
        HttpStatusCode.OK,
        second.StatusCode);

    var updated =
        await second.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralPriority.Urgent,
        updated.Priority);

    Assert.Equal(
        "Condition has deteriorated.",
        updated.TriageNote);
  }
}