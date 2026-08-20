using System.Security.Claims;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.Authentication;

public sealed class AdministrativeAccessPolicyTests
    : IClassFixture<
        CareTrackSqlServerWebApplicationFactory>
{
  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public AdministrativeAccessPolicyTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task
      AdministrativeAccess_WithAdministratorAndScope_Succeeds()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Administrator);

    using var scope =
        _factory.Services.CreateScope();

    var authorizationService =
        scope.ServiceProvider
            .GetRequiredService<
                IAuthorizationService>();

    // Act
    var result =
        await authorizationService
            .AuthorizeAsync(
                user,
                resource: null,
                CareTrackPolicies.AdministrativeAccess);

    // Assert
    Assert.True(
        result.Succeeded);
  }

  [Fact]
  public async Task
      AdministrativeAccess_WithClinicianAndScope_Fails()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    using var scope =
        _factory.Services.CreateScope();

    var authorizationService =
        scope.ServiceProvider
            .GetRequiredService<
                IAuthorizationService>();

    // Act
    var result =
        await authorizationService
            .AuthorizeAsync(
                user,
                resource: null,
                CareTrackPolicies.AdministrativeAccess);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  [Fact]
  public async Task
      AdministrativeAccess_WithReferralCoordinatorAndScope_Fails()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var scope =
        _factory.Services.CreateScope();

    var authorizationService =
        scope.ServiceProvider
            .GetRequiredService<
                IAuthorizationService>();

    // Act
    var result =
        await authorizationService
            .AuthorizeAsync(
                user,
                resource: null,
                CareTrackPolicies.AdministrativeAccess);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  [Fact]
  public async Task
      AdministrativeAccess_WithAdministratorButNoScope_Fails()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            scope: null,
            CareTrackRoles.Administrator);

    using var serviceScope =
        _factory.Services.CreateScope();

    var authorizationService =
        serviceScope.ServiceProvider
            .GetRequiredService<
                IAuthorizationService>();

    // Act
    var result =
        await authorizationService
            .AuthorizeAsync(
                user,
                resource: null,
                CareTrackPolicies.AdministrativeAccess);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  [Fact]
  public async Task
      AdministrativeAccess_WithScopeButNoRole_Fails()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            CareTrackScopes.AccessAsUser);

    using var scope =
        _factory.Services.CreateScope();

    var authorizationService =
        scope.ServiceProvider
            .GetRequiredService<
                IAuthorizationService>();

    // Act
    var result =
        await authorizationService
            .AuthorizeAsync(
                user,
                resource: null,
                CareTrackPolicies.AdministrativeAccess);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  private static ClaimsPrincipal
      CreateAuthenticatedUser(
          string? scope,
          params string[] roles)
  {
    var claims =
        new List<Claim>
        {
                new(
                    "oid",
                    "test-user-oid")
        };

    if (!string.IsNullOrWhiteSpace(scope))
    {
      claims.Add(
          new Claim(
              "scp",
              scope));
    }

    foreach (var role in roles)
    {
      claims.Add(
          new Claim(
              ClaimTypes.Role,
              role));
    }

    var identity =
        new ClaimsIdentity(
            claims,
            authenticationType: "Test");

    return new ClaimsPrincipal(
        identity);
  }
}