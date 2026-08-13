using CareTrack.Domain.Enums;

namespace CareTrack.Application.Referrals.CreateReferral;

public sealed record CreateReferralCommand(
    string ReferralReference,
    Guid PatientId,
    ReferralPriority Priority,
    string Reason);