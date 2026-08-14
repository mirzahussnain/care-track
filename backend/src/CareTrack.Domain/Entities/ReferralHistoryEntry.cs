using CareTrack.Domain.Enums;

namespace CareTrack.Domain.Entities;

public class ReferralHistoryEntry
{
  public Guid Id { get; private set; }
  public Guid ReferralId { get; private set; }

  public ReferralHistoryEventType EventType { get; private set; }

  public ReferralStatus? FromStatus { get; private set; }

  public ReferralStatus? ToStatus { get; private set; }

  public ReferralPriority? Priority { get; private set; }

  public string? TriageNote { get; private set; }

  public string? AssignedTo { get; private set; }

  public DateTime OccurredAt { get; private set; }

  private ReferralHistoryEntry() { }

  internal ReferralHistoryEntry(
        Guid referralId,
        ReferralHistoryEventType eventType,
        ReferralStatus? fromStatus,
        ReferralStatus? toStatus,
        DateTime occurredAt,
        ReferralPriority? priority = null,
        string? triageNote = null,
        string? assignedTo = null
  )
  {
    Id = Guid.NewGuid();

    ReferralId = referralId;
    EventType = eventType;

    FromStatus = fromStatus;
    ToStatus = toStatus;

    Priority = priority;
    TriageNote = triageNote;
    AssignedTo = assignedTo;

    OccurredAt = occurredAt;
  }
}