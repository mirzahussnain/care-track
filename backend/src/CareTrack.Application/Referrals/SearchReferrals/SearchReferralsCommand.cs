using CareTrack.Domain.Enums;

namespace CareTrack.Application.Referrals.SearchReferrals;

public sealed record SearchReferralsCommand(
    ReferralStatus? Status,
    ReferralPriority? Priority,
    Guid? PatientId,
    string? AssignedTo,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    string SortBy,
    string SortDirection,
    int Page,
    int PageSize);