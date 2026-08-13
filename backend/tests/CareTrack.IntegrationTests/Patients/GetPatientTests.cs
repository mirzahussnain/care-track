using System.Net;
using System.Net.Http.Json;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Patients;

public class GetPatientTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
    IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public GetPatientTests(
      CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task GetPatient_AfterCreation_ReturnsPatient()
  {
    var createRequest = new
    {
      patientReference = $"PAT-{Guid.NewGuid():N}"[..12],
      firstName = "Jane",
      lastName = "Taylor",
      dateOfBirth = "1992-08-15"
    };

    var createResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            createRequest);

    createResponse.EnsureSuccessStatusCode();

    var created =
        await createResponse.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(created);

    var response =
        await _client.GetAsync(
            $"/api/patients/{created.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var patient =
        await response.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(patient);
    Assert.Equal(created.Id, patient.Id);
  }

  [Fact]
  public async Task GetPatient_WithUnknownId_ReturnsNotFound()
  {
    var id = Guid.NewGuid();

    var response =
        await _client.GetAsync(
            $"/api/patients/{id}");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }
}