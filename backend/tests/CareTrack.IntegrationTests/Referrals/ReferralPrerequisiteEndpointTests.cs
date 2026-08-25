using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Referrals;

public sealed class ReferralPrerequisiteEndpointTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;
  private readonly HttpClient _coordinatorClient;

  public ReferralPrerequisiteEndpointTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
    _coordinatorClient = TestAuthenticatedClient.Create(
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
  public async Task ReferralPatientLookup_ForCoordinator_ReturnsReducedPagedContract()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _coordinatorClient);

    var response =
        await _coordinatorClient.GetAsync(
            "/api/patients/referral-lookup?search=Integration&page=1&pageSize=10");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result =
        await response.Content.ReadFromJsonAsync<
            PagedReferralPatientSummaryResponse>();

    Assert.NotNull(result);
    var summary = Assert.Single(result.Items);
    Assert.Equal(patient.Id, summary.Id);
    Assert.Equal(patient.PatientReference, summary.PatientReference);
    Assert.Equal(patient.FullName, summary.FullName);
    Assert.Equal(patient.DateOfBirth, summary.DateOfBirth);

    using var document =
        JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());
    var item =
        document.RootElement
            .GetProperty("items")[0];

    Assert.Equal(
        ["id", "patientReference", "fullName", "dateOfBirth"],
        item.EnumerateObject()
            .Select(property => property.Name)
            .ToArray());
  }

  [Fact]
  public async Task ReferralPatientSummary_ForCoordinator_ReturnsReducedContract()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _coordinatorClient);

    var response =
        await _coordinatorClient.GetAsync(
            $"/api/patients/{patient.Id}/referral-summary");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var summary =
        await response.Content.ReadFromJsonAsync<
            ReferralPatientSummaryResponse>();

    Assert.NotNull(summary);
    Assert.Equal(patient.Id, summary.Id);
    Assert.Equal(patient.PatientReference, summary.PatientReference);
    Assert.Equal(patient.FullName, summary.FullName);
    Assert.Equal(patient.DateOfBirth, summary.DateOfBirth);
  }

  [Fact]
  public async Task ExistingPatientDetail_ForCoordinator_RemainsForbidden()
  {
    var patient =
        await PatientApiTestHelper.CreatePatientAsync(
            _coordinatorClient);

    var response =
        await _coordinatorClient.GetAsync(
            $"/api/patients/{patient.Id}");

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }

  [Fact]
  public async Task ReferralPrerequisiteEndpoints_ForAdministrator_AreForbidden()
  {
    var administratorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.AdministratorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Administrator);

    var lookupResponse =
        await administratorClient.GetAsync(
            "/api/patients/referral-lookup");
    var targetsResponse =
        await administratorClient.GetAsync(
            "/api/referrals/assignment-targets");

    Assert.Equal(HttpStatusCode.Forbidden, lookupResponse.StatusCode);
    Assert.Equal(HttpStatusCode.Forbidden, targetsResponse.StatusCode);
  }

  [Fact]
  public async Task AssignmentTargets_ReturnConfiguredCanonicalNames()
  {
    var response =
        await _coordinatorClient.GetAsync(
            "/api/referrals/assignment-targets");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result =
        await response.Content.ReadFromJsonAsync<
            ReferralAssignmentTargetsResponse>();

    Assert.NotNull(result);
    Assert.Contains("Cardiology Team A", result.Items);
    Assert.Contains("Cardiology Team B", result.Items);
  }

  [Fact]
  public async Task Assign_WithDifferentCase_PersistsCanonicalConfiguredName()
  {
    var referral =
        await ReferralApiTestHelper.CreateAcceptedReferralAsync(
            _coordinatorClient);

    var response =
        await _coordinatorClient.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo = "  cardiology team a  "
            });

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var updated =
        await response.Content.ReadFromJsonAsync<
            ReferralResponse>();

    Assert.NotNull(updated);
    Assert.Equal("Cardiology Team A", updated.AssignedTo);
  }

  [Fact]
  public async Task Assign_WithUnavailableTarget_ReturnsBadRequest()
  {
    var referral =
        await ReferralApiTestHelper.CreateAcceptedReferralAsync(
            _coordinatorClient);

    var response =
        await _coordinatorClient.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo = "Unknown Team"
            });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }
}
