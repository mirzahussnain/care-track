namespace CareTrack.Api.Contracts.Referrals;

public sealed record PagedReferralResponse(
  IReadOnlyList<ReferralResponse> Items,
  int Page,
  int PageSize,
  int TotalCount,
  int TotalPages);