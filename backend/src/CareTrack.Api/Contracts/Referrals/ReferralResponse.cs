using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Referrals;

public sealed record ReferralResponse(
    Guid Id,
    string ReferralReference,
    Guid PatientId,
    ReferralStatus Status,
    ReferralPriority Priority,
    string Reason,
    string? TriageNote,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? UpdatedAt,
    DateTime? TriagedAt,
    string? AssignedTo,
    DateTime? AssignedAt);