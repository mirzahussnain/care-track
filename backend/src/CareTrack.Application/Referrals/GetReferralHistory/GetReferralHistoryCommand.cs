namespace CareTrack.Application.Referrals.GetReferralHistory;

public sealed record GetReferralHistoryCommand(
    Guid ReferralId);