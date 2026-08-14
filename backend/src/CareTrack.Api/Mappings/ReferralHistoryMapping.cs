using CareTrack.Api.Contracts.Referrals;
using CareTrack.Domain.Entities;

namespace CareTrack.Api.Mappings;

public static class ReferralHistoryMapping
{
  public static ReferralHistoryResponse ToResponse(
          this ReferralHistoryEntry history)
  {
    return new ReferralHistoryResponse(
        history.Id,
        history.EventType,
        history.FromStatus,
        history.ToStatus,
        history.Priority,
        history.TriageNote,
        history.AssignedTo,
        history.OccurredAt);
  }
}