using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Infrastructure;
namespace CareTrack.IntegrationTests.Referrals;

public class CreateReferralTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public CreateReferralTests(CareTrackSqlServerWebApplicationFactory factory)
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

  private async Task<PatientResponse>
       CreatePatientAsync()
  {
    var request = new
    {
      patientReference =
            $"PAT-{Guid.NewGuid():N}"[..12],

      firstName = "John",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
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

  [Fact]
  public async Task CreateReferral_WithValidRequest_ReturnsCreated()
  {
    // Arrange
    var patient =
        await CreatePatientAsync();

    var referralReference =
        $"REF-{Guid.NewGuid():N}"[..12];

    var request = new
    {
      referralReference,
      patientId = patient.Id,
      priority =
            ReferralPriority.Routine,
      reason =
            "Persistent shoulder pain requiring specialist assessment."
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var referral =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(referral);

    Assert.NotEqual(
        Guid.Empty,
        referral.Id);

    Assert.Equal(
        referralReference,
        referral.ReferralReference);

    Assert.Equal(
        patient.Id,
        referral.PatientId);

    Assert.Equal(
        ReferralPriority.Routine,
        referral.Priority);

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    Assert.Equal(
        "Persistent shoulder pain requiring specialist assessment.",
        referral.Reason);

    Assert.NotEqual(
        default,
        referral.CreatedAt);

    Assert.Null(
        referral.SubmittedAt);
  }

  [Fact]
  public async Task CreateReferral_WithUnknownPatient_ReturnsNotFound()
  {
    // Arrange
    var request = new
    {
      referralReference =
            $"REF-{Guid.NewGuid():N}"[..12],

      patientId =
            Guid.NewGuid(),

      priority =
            ReferralPriority.Routine,

      reason =
            "Routine specialist assessment."
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task CreateReferral_WithDuplicateReference_ReturnsConflict()
  {
    // Arrange
    var patient =
        await CreatePatientAsync();

    var referralReference =
        $"REF-{Guid.NewGuid():N}"[..12];

    var request = new
    {
      referralReference,
      patientId = patient.Id,
      priority =
            ReferralPriority.Routine,
      reason =
            "Initial referral."
    };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    // Act
    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        secondResponse.StatusCode);
  }

  [Fact]
  public async Task CreateReferral_WithBlankReason_ReturnsBadRequest()
  {
    // Arrange
    var patient =
        await CreatePatientAsync();

    var request = new
    {
      referralReference =
            $"REF-{Guid.NewGuid():N}"[..12],

      patientId = patient.Id,

      priority =
            ReferralPriority.Routine,

      reason = ""
    };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/referrals",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }
}
