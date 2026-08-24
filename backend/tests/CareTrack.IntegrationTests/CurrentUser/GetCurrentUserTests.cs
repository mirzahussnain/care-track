using System.Net;
using System.Net.Http.Json;

using CareTrack.IntegrationTests.Contracts.CurrentUser;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.CurrentUser;

public sealed class CurrentUserEndpointsTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public CurrentUserEndpointsTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task GetMe_WithValidAuthentication_ReturnsCurrentUser()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.CreateWithIdentity(
            _factory,
            TestUsers.ClinicianId,
            "access_as_user",
            "Test Clinician",
            "clinician@example.com",
            "Clinician");

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                CurrentUserResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        TestUsers.ClinicianId,
        result.Id);

    Assert.Equal(
        "Test Clinician",
        result.Name);

    Assert.Equal(
        "clinician@example.com",
        result.Username);

    Assert.Contains(
        "Clinician",
        result.Roles);
  }

  [Fact]
  public async Task GetMe_WithoutAuthentication_ReturnsUnauthorized()
  {
    // Arrange
    using var client =
        _factory.CreateClient();

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task GetMe_WithoutRequiredScope_ReturnsForbidden()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            scope: string.Empty,
            "Clinician");

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task GetMe_DoesNotRequireSpecificApplicationRole()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.AdministratorId,
            "access_as_user",
            "Administrator");

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                CurrentUserResponse>();

    Assert.NotNull(result);

    Assert.Contains(
        "Administrator",
        result.Roles);
  }

  [Fact]
  public async Task GetMe_WithMultipleRoles_ReturnsAllRoles()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            "access_as_user",
            "Clinician",
            "ReferralCoordinator");

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                CurrentUserResponse>();

    Assert.NotNull(result);

    Assert.Contains(
        "Clinician",
        result.Roles);

    Assert.Contains(
        "ReferralCoordinator",
        result.Roles);
  }

  [Fact]
  public async Task GetMe_WithScopeButNoApplicationRole_ReturnsOk()
  {
    // Arrange
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.BasicUserId,
            "access_as_user");

    // Act
    var response =
        await client.GetAsync("/api/me");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                CurrentUserResponse>();

    Assert.NotNull(result);

    Assert.Empty(
        result.Roles);
  }
}