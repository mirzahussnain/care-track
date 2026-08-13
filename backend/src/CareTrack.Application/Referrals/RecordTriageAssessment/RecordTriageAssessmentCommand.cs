using CareTrack.Domain.Enums;

namespace CareTrack.Application.Referrals.RecordTriageAssessment;

public sealed record RecordTriageAssessmentCommand(
    Guid ReferralId,
    ReferralPriority Priority,
    string Note);