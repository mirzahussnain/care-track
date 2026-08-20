using System.Net;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Authentication;

public sealed class AuthenticationSmokeTests
    : IClassFixture<
        CareTrackSqlServerWebApplicationFactory>
{
  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public AuthenticationSmokeTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory =
        factory;
  }

  [Fact]
  public async Task
      ProtectedEndpoint_WithoutAuthenticatedUser_ReturnsUnauthorized()
  {
    // Arrange
    using var client =
        _factory.CreateClient();

    // Act
    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task
      ProtectedEndpoint_WithClinicianIdentity_PassesAuthorization()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    // Act
    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }
}