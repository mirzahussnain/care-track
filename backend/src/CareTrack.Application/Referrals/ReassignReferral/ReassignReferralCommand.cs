namespace CareTrack.Application.Referrals.ReassignReferral;

public sealed record ReassignReferralCommand(
    Guid ReferralId,
    string AssignedTo);