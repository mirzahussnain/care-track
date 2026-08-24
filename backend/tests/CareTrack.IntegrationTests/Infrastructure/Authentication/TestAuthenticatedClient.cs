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

  public static HttpClient CreateWithIdentity(
     WebApplicationFactory<Program> factory,
     string userId,
     string scope,
     string name,
     string username,
     params string[] roles)
  {
    var client =
        CreateBaseClient(
            factory,
            userId,
            scope);

    if (!string.IsNullOrWhiteSpace(name))
    {
      client.DefaultRequestHeaders.Add(
          TestAuthenticationDefaults.NameHeader,
          name);
    }

    if (!string.IsNullOrWhiteSpace(username))
    {
      client.DefaultRequestHeaders.Add(
          TestAuthenticationDefaults.UsernameHeader,
          username);
    }

    AddRoles(
        client,
        roles);

    return client;
  }

  private static HttpClient CreateBaseClient(
      WebApplicationFactory<Program> factory,
      string userId,
      string scope)
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

    return client;
  }

  private static void AddRoles(
      HttpClient client,
      string[] roles)
  {
    if (roles.Length == 0)
    {
      return;
    }

    client.DefaultRequestHeaders.Add(
        TestAuthenticationDefaults.RolesHeader,
        string.Join(
            ",",
            roles));
  }
}


