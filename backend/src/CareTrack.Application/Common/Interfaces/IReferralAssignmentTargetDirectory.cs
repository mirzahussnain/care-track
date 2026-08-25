namespace CareTrack.Application.Common.Interfaces;

public interface IReferralAssignmentTargetDirectory
{
  IReadOnlyList<string> Targets { get; }

  string ResolveCanonicalName(string requestedName);
}
