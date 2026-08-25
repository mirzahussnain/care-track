namespace CareTrack.Api.Contracts.Referrals;

public sealed record ReferralAssignmentTargetsResponse(
    IReadOnlyList<string> Items);
