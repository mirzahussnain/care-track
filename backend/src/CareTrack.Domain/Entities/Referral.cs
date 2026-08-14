using CareTrack.Domain.Enums;

namespace CareTrack.Domain.Entities;

public class Referral
{
  public Guid Id { get; private set; }

  public string ReferralReference { get; private set; }

  public Guid PatientId { get; private set; }

  public ReferralStatus Status { get; private set; }

  public ReferralPriority Priority { get; private set; }

  public string Reason { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime? SubmittedAt { get; private set; }

  public DateTime? UpdatedAt { get; private set; }

  public string? TriageNote { get; private set; }

  public DateTime? TriagedAt { get; private set; }

  public string? AssignedTo { get; private set; }

  public DateTime? AssignedAt { get; private set; }

  private readonly List<ReferralHistoryEntry> _history = [];

  public IReadOnlyCollection<ReferralHistoryEntry> History => _history;
  private Referral()
  {
    ReferralReference = null!;
    Reason = null!;
  }

  public Referral(
      string referralReference,
      Guid patientId,
      ReferralPriority priority,
      string reason)
  {
    if (string.IsNullOrWhiteSpace(referralReference))
    {
      throw new ArgumentException(
          "Referral reference cannot be empty.",
          nameof(referralReference));
    }

    if (patientId == Guid.Empty)
    {
      throw new ArgumentException(
          "Patient ID cannot be empty.",
          nameof(patientId));
    }

    if (string.IsNullOrWhiteSpace(reason))
    {
      throw new ArgumentException(
          "Referral reason cannot be empty.",
          nameof(reason));
    }

    if (!Enum.IsDefined(priority))
    {
      throw new ArgumentException(
          "Referral priority is invalid.",
          nameof(priority));
    }

    if (reason.Trim().Length > 2000)
    {
      throw new ArgumentException(
          "Referral reason cannot exceed 2000 characters.",
          nameof(reason));
    }
    if (referralReference.Trim().Length > 30)
    {
      throw new ArgumentException(
          "Referral reference cannot exceed 30 characters.",
          nameof(referralReference));
    }

    var now = DateTime.UtcNow;

    Id = Guid.NewGuid();

    ReferralReference = referralReference.Trim();

    PatientId = patientId;

    Priority = priority;

    Reason = reason.Trim();

    Status = ReferralStatus.Draft;

    CreatedAt = now;

    AddHistory(
    ReferralHistoryEventType.Created,
    null,
    ReferralStatus.Draft,
    now,
    priority: Priority
    );
  }

  private void AddHistory(
    ReferralHistoryEventType eventType,
    ReferralStatus? fromStatus,
    ReferralStatus? toStatus,
    DateTime occurredAt,
    ReferralPriority? priority = null,
    string? triageNote = null,
    string? assignedTo = null)
  {
    _history.Add(new ReferralHistoryEntry(
            Id,
            eventType,
            fromStatus,
            toStatus,
            occurredAt,
            priority,
            triageNote,
            assignedTo));
  }

  private static string ValidateAssignmentTarget(
    string assignedTo)
  {
    if (string.IsNullOrWhiteSpace(assignedTo))
    {
      throw new ArgumentException(
          "Assignment target is required.",
          nameof(assignedTo));
    }

    if (assignedTo.Length > 200)
    {
      throw new ArgumentException(
          "Assignment target cannot exceed 200 characters.",
          nameof(assignedTo));
    }

    return assignedTo.Trim();
  }
  public void Submit()
  {
    if (Status != ReferralStatus.Draft)
    {
      throw new InvalidOperationException(
          "Only draft referrals can be submitted.");
    }

    var fromStatus = Status;

    var now = DateTime.UtcNow;


    Status = ReferralStatus.Submitted;
    SubmittedAt = now;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.Submitted,
    fromStatus,
    Status,
    now



    );
  }
  public void StartTriage()
  {
    if (Status != ReferralStatus.Submitted)
    {
      throw new InvalidOperationException(
          "Only submitted referrals can enter triage.");
    }

    var fromStatus = Status;
    var now = DateTime.UtcNow;

    Status = ReferralStatus.AwaitingTriage;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.TriageStarted,
    fromStatus,
    Status,
    now
    );
  }
  public void RequestMoreInformation()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "More information can only be requested for referrals awaiting triage.");
    }
    var fromStatus = Status;
    var now = DateTime.UtcNow;

    Status = ReferralStatus.MoreInformationRequired;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.MoreInformationRequested,
    fromStatus,
    Status,
    now
    );
  }
  public void Resubmit()
  {
    if (Status != ReferralStatus.MoreInformationRequired)
    {
      throw new InvalidOperationException(
          "Only referrals requiring more information can be resubmitted.");
    }
    var fromStatus = Status;
    var now = DateTime.UtcNow;

    Status = ReferralStatus.Submitted;
    UpdatedAt = now;

    AddHistory(
        ReferralHistoryEventType.Resubmitted,
        fromStatus,
        Status,
        now);
  }
  public void Accept()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "Only referrals awaiting triage can be accepted.");
    }

    var fromStatus = Status;
    var now = DateTime.UtcNow;

    Status = ReferralStatus.Accepted;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.Accepted,
    fromStatus,
    Status,
    now);

  }
  public void Reject()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "Only referrals awaiting triage can be rejected.");
    }

    var fromStatus = Status;
    var now = DateTime.UtcNow;

    Status = ReferralStatus.Rejected;
    UpdatedAt = now;

    AddHistory(
ReferralHistoryEventType.Rejected,
fromStatus,
Status,
now);
  }

  public void RecordTriageAssessment(
    ReferralPriority priority,
    string note)
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "Triage assessment can only be recorded for referrals awaiting triage.");
    }

    if (!Enum.IsDefined(priority))
    {
      throw new ArgumentOutOfRangeException(
          nameof(priority),
          "Invalid referral priority.");
    }

    if (string.IsNullOrWhiteSpace(note))
    {
      throw new ArgumentException(
          "Triage note is required.",
          nameof(note));
    }

    if (note.Length > 2000)
    {
      throw new ArgumentException(
          "Triage note cannot exceed 2000 characters.",
          nameof(note));
    }

    var now = DateTime.UtcNow;

    Priority = priority;
    TriageNote = note.Trim();
    TriagedAt = now;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.TriageAssessmentRecorded,
    Status,
    Status,
    now,
    priority: Priority,
    triageNote: TriageNote);
  }

  public void Assign(string assignedTo)
  {
    if (Status != ReferralStatus.Accepted)
    {
      throw new InvalidOperationException(
          "Only accepted referrals can be assigned.");
    }
    var normalizedAssignedTo = ValidateAssignmentTarget(assignedTo);

    var now = DateTime.UtcNow;
    var fromStatus = Status;

    AssignedTo = normalizedAssignedTo.Trim();
    AssignedAt = now;
    Status = ReferralStatus.Assigned;
    UpdatedAt = now;

    AddHistory(
    ReferralHistoryEventType.Assigned,
    fromStatus,
    Status,
    now,
    assignedTo: AssignedTo);
  }

  public void Reassign(string assignedTo)
  {
    if (Status != ReferralStatus.Assigned)
    {
      throw new InvalidOperationException(
          "Only assigned referrals can be reassigned.");
    }
    var normalizedAssignedTo = ValidateAssignmentTarget(assignedTo);

    var now = DateTime.UtcNow;

    AssignedTo = normalizedAssignedTo.Trim();
    AssignedAt = now;
    UpdatedAt = now;

    AddHistory(
       ReferralHistoryEventType.Reassigned,
       Status,
       Status,
       now,
       assignedTo: AssignedTo);
  }
}