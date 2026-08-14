using CareTrack.Domain.Enums;
namespace CareTrack.IntegrationTests.Contracts.Referrals;

public sealed record ReferralResponse(
    Guid Id,
    string ReferralReference,
    Guid PatientId,
    ReferralStatus Status,
    ReferralPriority Priority,
    string Reason,
    DateTime CreatedAt,
    DateTime? SubmittedAt,
    DateTime? UpdatedAt,
    string? TriageNote,
    DateTime? TriagedAt,
    string? AssignedTo,
    DateTime? AssignedAt);
