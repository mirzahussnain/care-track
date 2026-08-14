namespace CareTrack.Application.Referrals.AssignReferral;

public sealed record AssignReferralCommand(
    Guid ReferralId,
    string AssignedTo);