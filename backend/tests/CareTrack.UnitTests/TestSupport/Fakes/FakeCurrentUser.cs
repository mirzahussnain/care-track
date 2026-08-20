using CareTrack.Application.Common.Interfaces;

namespace CareTrack.UnitTests.TestSupport.Fakes;

public sealed class FakeCurrentUser(string userId = "test-user-id") : ICurrentUser
{
  public string UserId { get; } = userId;
}

