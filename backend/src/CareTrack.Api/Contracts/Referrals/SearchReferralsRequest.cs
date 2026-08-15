using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Referrals;

public sealed class SearchReferralRequest
{
  public ReferralStatus? Status { get; init; }

  public ReferralPriority? Priority { get; init; }

  public Guid? PatientId { get; init; }

  public string? AssignedTo { get; init; }

  public DateOnly? CreatedFrom { get; init; }

  public DateOnly? CreatedTo { get; init; }

  public string SortBy { get; init; } = "createdAt";

  public string SortDirection { get; init; } = "desc";
  public int Page { get; init; } = 1;

  public int PageSize { get; init; } = 20;


}