using CareTrack.Application.Common.Interfaces;

namespace CareTrack.Infrastructure.Configuration;

public sealed class ConfiguredReferralAssignmentTargetDirectory
    : IReferralAssignmentTargetDirectory
{
  private readonly IReadOnlyList<string> _targets;

  public ConfiguredReferralAssignmentTargetDirectory(
      IEnumerable<string> configuredTargets)
  {
    var targets = configuredTargets
        .Select(target => target?.Trim() ?? string.Empty)
        .ToArray();

    if (targets.Length == 0)
    {
      throw new InvalidOperationException(
          "At least one referral assignment target must be configured.");
    }

    if (targets.Any(string.IsNullOrWhiteSpace))
    {
      throw new InvalidOperationException(
          "Referral assignment targets cannot be blank.");
    }

    if (targets.Any(target => target.Length > 200))
    {
      throw new InvalidOperationException(
          "Referral assignment targets cannot exceed 200 characters.");
    }

    if (targets.Distinct(StringComparer.OrdinalIgnoreCase).Count() != targets.Length)
    {
      throw new InvalidOperationException(
          "Referral assignment targets must be unique ignoring case.");
    }

    _targets = targets;
  }

  public IReadOnlyList<string> Targets => _targets;

  public string ResolveCanonicalName(string requestedName)
  {
    if (string.IsNullOrWhiteSpace(requestedName))
    {
      throw new ArgumentException(
          "Assignment target is required.",
          nameof(requestedName));
    }

    var normalizedName = requestedName.Trim();
    var canonicalName = _targets.FirstOrDefault(
        target => string.Equals(
            target,
            normalizedName,
            StringComparison.OrdinalIgnoreCase));

    return canonicalName
        ?? throw new ArgumentException(
            "The selected assignment target is not available.",
            nameof(requestedName));
  }
}
