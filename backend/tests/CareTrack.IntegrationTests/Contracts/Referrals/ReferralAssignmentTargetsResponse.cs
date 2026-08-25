namespace CareTrack.IntegrationTests.Contracts.Referrals;

public sealed record ReferralAssignmentTargetsResponse(
    IReadOnlyList<string> Items);
