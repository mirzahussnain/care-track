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

    Id = Guid.NewGuid();

    ReferralReference =
        referralReference.Trim();

    PatientId =
        patientId;

    Priority =
        priority;

    Reason =
        reason.Trim();

    Status =
        ReferralStatus.Draft;

    CreatedAt =
        DateTime.UtcNow;
  }
  public void Submit()
  {
    if (Status != ReferralStatus.Draft)
    {
      throw new InvalidOperationException(
          "Only draft referrals can be submitted.");
    }

    Status =
        ReferralStatus.Submitted;

    var now = DateTime.UtcNow;

    Status = ReferralStatus.Submitted;
    SubmittedAt = now;
    UpdatedAt = now;
  }
  public void StartTriage()
  {
    if (Status != ReferralStatus.Submitted)
    {
      throw new InvalidOperationException(
          "Only submitted referrals can enter triage.");
    }

    Status = ReferralStatus.AwaitingTriage;
    UpdatedAt = DateTime.UtcNow;
  }
  public void RequestMoreInformation()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "More information can only be requested for referrals awaiting triage.");
    }

    Status = ReferralStatus.MoreInformationRequired;
    UpdatedAt = DateTime.UtcNow;
  }
  public void Resubmit()
  {
    if (Status != ReferralStatus.MoreInformationRequired)
    {
      throw new InvalidOperationException(
          "Only referrals requiring more information can be resubmitted.");
    }

    Status = ReferralStatus.Submitted;
    UpdatedAt = DateTime.UtcNow;
  }
  public void Accept()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "Only referrals awaiting triage can be accepted.");
    }

    Status = ReferralStatus.Accepted;
    UpdatedAt = DateTime.UtcNow;
  }
  public void Reject()
  {
    if (Status != ReferralStatus.AwaitingTriage)
    {
      throw new InvalidOperationException(
          "Only referrals awaiting triage can be rejected.");
    }

    Status = ReferralStatus.Rejected;
    UpdatedAt = DateTime.UtcNow;
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
  }
}