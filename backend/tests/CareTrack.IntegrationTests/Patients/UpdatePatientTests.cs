using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Patients;

public class UpdatePatientTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public UpdatePatientTests(CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
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
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    var createRequest = new
    {
      patientReference = $"PAT-{Guid.NewGuid():N}"[..12],
      firstName = "John",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    var createResponse =
        await referralCoordinatorClient.PostAsJsonAsync(
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
        await referralCoordinatorClient.PutAsJsonAsync(
            $"/api/patients/{created.Id}",
            updateRequest);

    Assert.Equal(
        HttpStatusCode.OK,
        updateResponse.StatusCode);

    var getResponse =
        await clinicianClient.GetAsync(
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