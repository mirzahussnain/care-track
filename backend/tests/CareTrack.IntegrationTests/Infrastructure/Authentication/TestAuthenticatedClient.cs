using Microsoft.AspNetCore.Mvc.Testing;

namespace CareTrack.IntegrationTests.Infrastructure.Authentication;

public static class TestAuthenticatedClient
{
  public static HttpClient Create(
      WebApplicationFactory<Program> factory,
      string userId,
      string scope,
      params string[] roles)
  {
    var client =
        factory.CreateClient();

    client.DefaultRequestHeaders.Add(
        TestAuthenticationDefaults.UserIdHeader,
        userId);

    if (!string.IsNullOrWhiteSpace(scope))
    {
      client.DefaultRequestHeaders.Add(
          TestAuthenticationDefaults.ScopeHeader,
          scope);
    }

    if (roles.Length > 0)
    {
      client.DefaultRequestHeaders.Add(
          TestAuthenticationDefaults.RolesHeader,
          string.Join(
              ",",
              roles));
    }

    return client;
  }
}