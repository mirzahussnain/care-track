using CareTrack.Domain.Enums;

namespace CareTrack.Application.Referrals.SearchReferrals;

public sealed record ReferralSearchCriteria(
    ReferralStatus? Status,
    ReferralPriority? Priority,
    Guid? PatientId,
    string? AssignedTo,
    DateTime? CreatedFrom,
    DateTime? CreatedTo,
    ReferralSortField SortField,
    SortDirection SortDirection,
    int Page,
    int PageSize);