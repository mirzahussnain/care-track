using System.Security.Claims;
using CareTrack.Api.Authorization;
using CareTrack.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CareTrack.IntegrationTests.Authentication;

public sealed class ReferralManagementPolicyTests
    : IClassFixture<
        CareTrackSqlServerWebApplicationFactory>
{
  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public ReferralManagementPolicyTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task
      ReferralManagement_WithReferralCoordinatorAndScope_Succeeds()
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
                CareTrackPolicies.ReferralManagement);

    // Assert
    Assert.True(
        result.Succeeded);
  }

  [Fact]
  public async Task
      ReferralManagement_WithClinicianAndScope_Succeeds()
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
                CareTrackPolicies.ReferralManagement);

    // Assert
    Assert.True(
        result.Succeeded);
  }

  [Fact]
  public async Task
      ReferralManagement_WithAdministratorAndScope_Fails()
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
                CareTrackPolicies.ReferralManagement);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  [Fact]
  public async Task
      ReferralManagement_WithScopeButNoRole_Fails()
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
                CareTrackPolicies.ReferralManagement);

    // Assert
    Assert.False(
        result.Succeeded);
  }

  [Fact]
  public async Task
      ReferralManagement_WithRoleButNoScope_Fails()
  {
    // Arrange
    var user =
        CreateAuthenticatedUser(
            scope: null,
            CareTrackRoles.ReferralCoordinator);

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
                CareTrackPolicies.ReferralManagement);

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