using System.Net;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Authentication;

public sealed class ClinicianAccessPolicyTests
    : IClassFixture<
        CareTrackSqlServerWebApplicationFactory>
{
  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public ClinicianAccessPolicyTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task
      ClinicianAccess_WhenUnauthenticated_ReturnsUnauthorized()
  {
    using var client =
        _factory.CreateClient();

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task
      ClinicianAccess_WhenScopeIsMissing_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            scope: string.Empty,
            CareTrackRoles.Clinician);

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      ClinicianAccess_WhenRoleIsMissing_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.BasicUserId,
            CareTrackScopes.AccessAsUser);

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      ClinicianAccess_WithScopeAndClinicianRole_ReachesEndpoint()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }
}