namespace CareTrack.IntegrationTests.Contracts.CurrentUser;

public sealed record CurrentUserResponse(
    string Id,
    string Name,
    string Username,
    IReadOnlyCollection<string> Roles);