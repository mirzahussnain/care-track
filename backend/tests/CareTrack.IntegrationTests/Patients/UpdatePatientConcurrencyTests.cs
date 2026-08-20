using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;
namespace CareTrack.IntegrationTests.Patients;

public class UpdatePatientConcurrencyTests
    : IClassFixture<
        CareTrackSqlServerWebApplicationFactory>,
        IAsyncLifetime

{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public UpdatePatientConcurrencyTests(
      CareTrackSqlServerWebApplicationFactory factory)
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
  public async Task UpdatePatient_WithCurrentRowVersion_ReturnsOk()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    // Arrange
    var patient =
     await PatientApiTestHelper
         .CreatePatientAsync(referralCoordinatorClient);

    var request = new
    {
      firstName =
            "Adam",

      lastName =
            "Jones",

      dateOfBirth =
            "1990-05-20",

      rowVersion =
            patient.RowVersion
    };

    // Act
    var response =
        await referralCoordinatorClient.PutAsJsonAsync(
            $"/api/patients/{patient.Id}",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var updated =
    await response.Content
    .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(updated);

    Assert.Equal("Adam", updated.FirstName);
    Assert.Equal("Jones", updated.LastName);

    Assert.NotEqual(
        patient.RowVersion,
        updated.RowVersion);


  }

  [Fact]
  public async Task UpdatePatient_WithStaleRowVersion_ReturnsConflict()
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

    // Arrange
    var patient =
     await PatientApiTestHelper
         .CreatePatientAsync(referralCoordinatorClient);

    var originalRowVersion =
        patient.RowVersion;

    // First user updates successfully.
    var firstUpdate = new
    {
      firstName =
            "Adam",

      lastName =
            "Jones",

      dateOfBirth =
            "1990-05-20",

      rowVersion =
            originalRowVersion
    };

    var firstResponse =
        await referralCoordinatorClient.PutAsJsonAsync(
            $"/api/patients/{patient.Id}",
            firstUpdate);

    Assert.Equal(
        HttpStatusCode.OK,
        firstResponse.StatusCode);

    // Second user still has the OLD row version.
    var staleUpdate = new
    {
      firstName =
            "Adam",

      lastName =
            "Brown",

      dateOfBirth =
            "1990-05-20",

      rowVersion =
            originalRowVersion
    };

    // Act
    var staleResponse =
        await referralCoordinatorClient.PutAsJsonAsync(
            $"/api/patients/{patient.Id}",
            staleUpdate);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        staleResponse.StatusCode);
    var body =
    await staleResponse.Content
        .ReadAsStringAsync();

    Assert.Contains(
        "modified by another user",
        body,
        StringComparison.OrdinalIgnoreCase);

    var getResponse =
    await clinicianClient.GetAsync(
        $"/api/patients/{patient.Id}");

    Assert.Equal(
        HttpStatusCode.OK,
        getResponse.StatusCode);

    var persisted =
        await getResponse.Content
            .ReadFromJsonAsync<PatientResponse>();

    Assert.NotNull(persisted);

    Assert.Equal(
        "Jones",
        persisted.LastName);
  }

  [Fact]
  public async Task UpdatePatient_WithInvalidBase64RowVersion_ReturnsBadRequest()
  {
    using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var patient =
    await PatientApiTestHelper
        .CreatePatientAsync(referralCoordinatorClient);

    var request = new
    {
      firstName =
            "Adam",

      lastName =
            "Jones",

      dateOfBirth =
            "1990-05-20",

      rowVersion =
            "not-valid-base64!!!"
    };

    var response =
        await referralCoordinatorClient.PutAsJsonAsync(
            $"/api/patients/{patient.Id}",
            request);

    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }
}