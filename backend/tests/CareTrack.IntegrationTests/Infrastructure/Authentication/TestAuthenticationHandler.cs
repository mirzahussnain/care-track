using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CareTrack.IntegrationTests.Infrastructure.Authentication;

public sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
  public TestAuthenticationHandler(
      IOptionsMonitor<AuthenticationSchemeOptions> options,
      ILoggerFactory logger,
      UrlEncoder encoder)
      : base(
          options,
          logger,
          encoder)
  {
  }

  protected override Task<AuthenticateResult>
      HandleAuthenticateAsync()
  {
    if (!Request.Headers.TryGetValue(
            TestAuthenticationDefaults.UserIdHeader,
            out var userIdValues))
    {
      return Task.FromResult(
          AuthenticateResult.NoResult());
    }

    var userId =
        userIdValues.ToString();

    if (string.IsNullOrWhiteSpace(userId))
    {
      return Task.FromResult(
          AuthenticateResult.NoResult());
    }

    var claims =
        new List<Claim>
        {
                new(
                    "oid",
                    userId)
        };

    if (Request.Headers.TryGetValue(
            TestAuthenticationDefaults.ScopeHeader,
            out var scopeValues))
    {
      var scope =
          scopeValues.ToString();

      if (!string.IsNullOrWhiteSpace(scope))
      {
        claims.Add(
            new Claim(
                "scp",
                scope));
      }
    }

    if (Request.Headers.TryGetValue(
            TestAuthenticationDefaults.RolesHeader,
            out var roleValues))
    {
      var roles =
          roleValues
              .ToString()
              .Split(
                  ',',
                  StringSplitOptions.RemoveEmptyEntries |
                  StringSplitOptions.TrimEntries);

      foreach (var role in roles)
      {
        claims.Add(
            new Claim(
                ClaimTypes.Role,
                role));
      }
    }

    var identity =
        new ClaimsIdentity(
            claims,
            TestAuthenticationDefaults
                .AuthenticationScheme);

    var principal =
        new ClaimsPrincipal(
            identity);

    var ticket =
        new AuthenticationTicket(
            principal,
            TestAuthenticationDefaults
                .AuthenticationScheme);

    return Task.FromResult(
        AuthenticateResult.Success(
            ticket));
  }
}