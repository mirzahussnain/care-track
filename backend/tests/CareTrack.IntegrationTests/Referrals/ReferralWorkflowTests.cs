using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Referrals;

public class ReferralWorkflowTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public ReferralWorkflowTests(CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task SubmitReferral_WhenDraft_ReturnsSubmittedReferral()
  {
    // Arrange
    var referral =
        await ReferralApiTestHelper.CreateReferralAsync(_client);

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    // Act
    var response =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/submit",
            content: null);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var updated =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralStatus.Submitted,
        updated.Status);

    Assert.NotNull(
        updated.SubmittedAt);
  }

  [Fact]
  public async Task SubmitReferral_WithUnknownReferral_ReturnsNotFound()
  {
    var response =
        await _client.PostAsync(
            $"/api/referrals/{Guid.NewGuid()}/submit",
            null);

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task StartTriage_WhenSubmitted_ReturnsAwaitingTriage()
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

    var updated =
        await triageResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralStatus.AwaitingTriage,
        updated.Status);
  }

  [Fact]
  public async Task StartTriage_WhenDraft_ReturnsConflict()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    var response =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/start-triage",
            null);

    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task AcceptReferral_WhenAwaitingTriage_ReturnsAccepted()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/submit",
        null);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/start-triage",
        null);

    var response =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/accept",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var updated =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralStatus.Accepted,
        updated.Status);
  }

  [Fact]
  public async Task Referral_CanRequestMoreInformationAndBeResubmitted()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/submit",
        null);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/start-triage",
        null);

    var infoResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/request-more-information",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        infoResponse.StatusCode);

    var infoReferral =
        await infoResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(infoReferral);

    Assert.Equal(
        ReferralStatus.MoreInformationRequired,
        infoReferral.Status);

    var originalSubmittedAt =
        infoReferral.SubmittedAt;

    var resubmitResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/resubmit",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        resubmitResponse.StatusCode);

    var resubmitted =
        await resubmitResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(resubmitted);

    Assert.Equal(
        ReferralStatus.Submitted,
        resubmitted.Status);

    Assert.Equal(
        originalSubmittedAt,
        resubmitted.SubmittedAt);
  }

  [Fact]
  public async Task RejectReferral_WhenAwaitingTriage_ReturnsRejected()
  {
    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/submit",
        null);

    await _client.PostAsync(
        $"/api/referrals/{referral.Id}/start-triage",
        null);

    var response =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/reject",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var updated =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralStatus.Rejected,
        updated.Status);
  }

}