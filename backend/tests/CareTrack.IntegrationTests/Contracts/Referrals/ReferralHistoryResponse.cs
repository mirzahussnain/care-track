using CareTrack.Domain.Enums;

namespace CareTrack.IntegrationTests.Contracts.Referrals;

public sealed record ReferralHistoryResponse(
    Guid Id,
    ReferralHistoryEventType EventType,
    ReferralStatus? FromStatus,
    ReferralStatus? ToStatus,
    ReferralPriority? Priority,
    string? TriageNote,
    string? AssignedTo,
    DateTime OccurredAt);