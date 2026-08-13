using CareTrack.Api.Contracts.Referrals;
using CareTrack.Domain.Entities;

namespace CareTrack.Api.Mappings;

public static class ReferralMappings
{
  public static ReferralResponse ToResponse(
      this Referral referral)
  {
    return new ReferralResponse(
        referral.Id,
        referral.ReferralReference,
        referral.PatientId,
        referral.Status,
        referral.Priority,
        referral.Reason,
        referral.TriageNote,
        referral.CreatedAt,
        referral.SubmittedAt,
        referral.UpdatedAt,
        referral.TriagedAt);
  }
}