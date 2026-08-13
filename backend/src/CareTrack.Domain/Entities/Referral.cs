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

    SubmittedAt =
        DateTime.UtcNow;

    UpdatedAt =
        DateTime.UtcNow;
  }
}