namespace CareTrack.Application.Referrals.RejectReferral;

public sealed record RejectReferralCommand(
    Guid ReferralId);
