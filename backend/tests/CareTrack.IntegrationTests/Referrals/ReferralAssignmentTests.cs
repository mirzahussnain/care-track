using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Referrals;

public class ReferralAssignmentTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public ReferralAssignmentTests(CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task AssignReferral_WhenAccepted_ReturnsAssignedReferral()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var request = new
    {
      assignedTo =
            "Cardiology Team A"
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
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
        ReferralStatus.Assigned,
        updated.Status);

    Assert.Equal(
        "Cardiology Team A",
        updated.AssignedTo);

    Assert.NotNull(
        updated.AssignedAt);

    Assert.NotNull(
        updated.UpdatedAt);
  }

  [Fact]
  public async Task AssignReferral_WhenDraft_ReturnsConflict()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateReferralAsync();

    var request = new
    {
      assignedTo =
            "Cardiology Team A"
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task AssignReferral_WithUnknownReferral_ReturnsNotFound()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var unknownReferralId =
        Guid.NewGuid();

    var request = new
    {
      assignedTo =
            "Cardiology Team A"
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{unknownReferralId}/assign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task AssignReferral_WithBlankAssignmentTarget_ReturnsBadRequest()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var request = new
    {
      assignedTo = ""
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task AssignReferral_WithAssignmentTargetLongerThan200Characters_ReturnsBadRequest()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var request = new
    {
      assignedTo =
            new string('A', 201)
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task ReassignReferral_WhenAssigned_ReturnsUpdatedAssignment()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var assignResponse =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo =
                    "Cardiology Team A"
            });

    Assert.Equal(
        HttpStatusCode.OK,
        assignResponse.StatusCode);

    var assignedReferral =
        await assignResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        assignedReferral);

    Assert.Equal(
        ReferralStatus.Assigned,
        assignedReferral.Status);

    Assert.Equal(
        "Cardiology Team A",
        assignedReferral.AssignedTo);

    var firstAssignedAt =
        assignedReferral.AssignedAt;

    // Act
    var reassignResponse =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/reassign",
            new
            {
              assignedTo =
                    "Cardiology Team B"
            });

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        reassignResponse.StatusCode);

    var updated =
        await reassignResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(updated);

    Assert.Equal(
        ReferralStatus.Assigned,
        updated.Status);

    Assert.Equal(
        "Cardiology Team B",
        updated.AssignedTo);

    Assert.NotNull(
        updated.AssignedAt);

    Assert.True(
        updated.AssignedAt >= firstAssignedAt);
  }

  [Fact]
  public async Task ReassignReferral_WhenAcceptedButNotAssigned_ReturnsConflict()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var request = new
    {
      assignedTo =
            "Cardiology Team B"
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/reassign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        response.StatusCode);
  }

  [Fact]
  public async Task ReassignReferral_WithUnknownReferral_ReturnsNotFound()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var request = new
    {
      assignedTo =
            "Cardiology Team B"
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{Guid.NewGuid()}/reassign",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task ReassignReferral_WithBlankAssignmentTarget_ReturnsBadRequest()
  {
    // Arrange
    await _factory.ResetDatabaseAsync();

    var referral =
        await CreateAcceptedReferralAsync();

    var assignResponse =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo =
                    "Cardiology Team A"
            });

    Assert.Equal(
        HttpStatusCode.OK,
        assignResponse.StatusCode);

    // Act
    var reassignResponse =
        await _client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/reassign",
            new
            {
              assignedTo = ""
            });

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        reassignResponse.StatusCode);
  }

  private async Task<ReferralResponse>
      CreateAcceptedReferralAsync()
  {
    // Create referral in Draft state.
    var referral =
        await CreateReferralAsync();

    // Draft -> Submitted
    var submitResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/submit",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        submitResponse.StatusCode);

    var submitted =
        await submitResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(submitted);

    Assert.Equal(
        ReferralStatus.Submitted,
        submitted.Status);

    // Submitted -> AwaitingTriage
    var startTriageResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/start-triage",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        startTriageResponse.StatusCode);

    var awaitingTriage =
        await startTriageResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(awaitingTriage);

    Assert.Equal(
        ReferralStatus.AwaitingTriage,
        awaitingTriage.Status);

    // AwaitingTriage -> Accepted
    var acceptResponse =
        await _client.PostAsync(
            $"/api/referrals/{referral.Id}/accept",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        acceptResponse.StatusCode);

    var accepted =
        await acceptResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(accepted);

    Assert.Equal(
        ReferralStatus.Accepted,
        accepted.Status);

    return accepted;
  }

  private async Task<ReferralResponse>
      CreateReferralAsync()
  {
    var patient =
        await CreatePatientAsync();

    var referralReference =
        $"REF-{Guid.NewGuid():N}"[..20];

    var request = new
    {
      referralReference,
      patientId =
            patient.Id,

      priority =
            ReferralPriority.Routine,

      reason =
            "Patient requires specialist assessment."
    };

    var response =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var referral =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(referral);

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    return referral;
  }

  private async Task<PatientResponse>
      CreatePatientAsync()
  {
    var patientReference =
        $"PAT-{Guid.NewGuid():N}"[..20];

    var request = new
    {
      patientReference,

      firstName =
            "Integration",

      lastName =
            "Patient",

      dateOfBirth =
            "1990-05-15"
    };

    var response =
        await _client.PostAsJsonAsync(
            "/api/patients",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var patient =
        await response.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(patient);

    return patient;
  }


}