using System.Net;
using System.Net.Http.Json;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Patients;

public class UpdatePatientTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public UpdatePatientTests(CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task UpdatePatient_WithValidRequest_PersistsChanges()
  {
    var createRequest = new
    {
      patientReference = $"PAT-{Guid.NewGuid():N}"[..12],
      firstName = "John",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    var createResponse =
        await _client.PostAsJsonAsync(
            "/api/patients",
            createRequest);


    Assert.Equal(
        HttpStatusCode.Created,
        createResponse.StatusCode);
    var created =
        await createResponse.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(created);

    var updateRequest = new
    {
      firstName = "Adam",
      lastName = "Jones",
      dateOfBirth = "1991-06-15",
      rowVersion = created.RowVersion,
    };

    var updateResponse =
        await _client.PutAsJsonAsync(
            $"/api/patients/{created.Id}",
            updateRequest);

    Assert.Equal(
        HttpStatusCode.OK,
        updateResponse.StatusCode);

    var getResponse =
        await _client.GetAsync(
            $"/api/patients/{created.Id}");

    Assert.Equal(
                HttpStatusCode.OK,
                getResponse.StatusCode);

    var updated =
        await getResponse.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(updated);
    Assert.Equal("Adam", updated.FirstName);
    Assert.Equal("Jones", updated.LastName);
    Assert.Equal(new DateOnly(1991, 6, 15), updated.DateOfBirth);
  }
}