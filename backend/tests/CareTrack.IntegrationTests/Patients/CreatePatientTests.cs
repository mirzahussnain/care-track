using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;
namespace CareTrack.IntegrationTests.Patients;

public class CreatePatientTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
    IAsyncLifetime

{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public CreatePatientTests(
      CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task CreatePatient_WithValidRequest_ReturnsCreated()
  {
    // Arrange
    var request = new
    {
      patientReference = $"PAT-{Guid.NewGuid():N}"[..12],
      firstName = "John",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    // Act
    var response = await _client.PostAsJsonAsync(
        "/api/patients",
        request);
    // Assert
    Assert.Equal(
    HttpStatusCode.Created,
    response.StatusCode);


    var patient = await response.Content.ReadFromJsonAsync<PatientResponse>();

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
    Assert.NotNull(patient);
    Assert.Equal("John", patient.FirstName);
    Assert.Equal("Smith", patient.LastName);
    Assert.NotNull(response.Headers.Location);
  }



  [Fact]
  public async Task CreatePatient_WithDuplicateReference_ReturnsConflict()
  {
    var patientReference =
        $"PAT-{Guid.NewGuid():N}"[..12];

    var request = new
    {
      patientReference,
      firstName = "John",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            request);

    Assert.Equal(
        HttpStatusCode.Conflict,
        secondResponse.StatusCode);
  }


  [Fact]
  public async Task CreatePatient_WithWhitespaceNormalizedDuplicateReference_ReturnsConflict()
  {
    var patientReference =
        $"PAT-{Guid.NewGuid():N}"[..12];

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            new
            {
              patientReference,
              firstName = "John",
              lastName = "Smith",
              dateOfBirth = "1990-05-20"
            });

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    var duplicateResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            new
            {
              patientReference = $" {patientReference} ",
              firstName = "Jane",
              lastName = "Smith",
              dateOfBirth = "1991-05-20"
            });

    Assert.Equal(
        HttpStatusCode.Conflict,
        duplicateResponse.StatusCode);
  }

  [Fact]
  public async Task CreatePatient_WithInValidBody_ReturnsBadRequest()
  {
    var request = new
    {
      patientReference = "PAT-INV-001",
      firstName = "",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    var response =
        await _client.PostAsJsonAsync(
            "/api/patients",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

}