using CareTrack.Application.Common.Interfaces;

namespace CareTrack.UnitTests.TestSupport.Fakes;

public sealed class FakeReferralAssignmentTargetDirectory(
    params string[] targets)
    : IReferralAssignmentTargetDirectory
{
  private readonly IReadOnlyList<string> _targets =
      targets.Length > 0
          ? targets
          : ["Cardiology Team A", "Cardiology Team B"];

  public IReadOnlyList<string> Targets => _targets;

  public string ResolveCanonicalName(string requestedName)
  {
    if (string.IsNullOrWhiteSpace(requestedName))
    {
      throw new ArgumentException(
          "Assignment target is required.",
          nameof(requestedName));
    }

    var canonicalName = _targets.FirstOrDefault(
        target => string.Equals(
            target,
            requestedName.Trim(),
            StringComparison.OrdinalIgnoreCase));

    return canonicalName
        ?? throw new ArgumentException(
            "The selected assignment target is not available.",
            nameof(requestedName));
  }
}
