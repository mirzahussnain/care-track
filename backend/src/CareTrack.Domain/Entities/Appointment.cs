using CareTrack.Domain.Enums;

namespace CareTrack.Domain.Entities;

public class Appointment
{
  public Guid Id { get; private set; }

  public string AppointmentReference { get; private set; } = string.Empty;

  public Guid PatientId
  {
    get;
    private set;
  }

  public Guid ReferralId
  {
    get;
    private set;
  }

  public AppointmentType AppointmentType
  {
    get;
    private set;
  }

  public DateTime ScheduledStart
  {
    get;
    private set;
  }

  public DateTime ScheduledEnd
  {
    get;
    private set;
  }

  public string Location
  {
    get;
    private set;
  } = string.Empty;

  public AppointmentStatus Status
  {
    get;
    private set;
  }

  public DateTime CreatedAt
  {
    get;
    private set;
  }

  public DateTime? UpdatedAt
  {
    get;
    private set;
  }

  public DateTime? CheckedInAt
  {
    get;
    private set;
  }

  public DateTime? StartedAt
  {
    get;
    private set;
  }

  public DateTime? CompletedAt
  {
    get;
    private set;
  }

  public DateTime? CancelledAt
  {
    get;
    private set;
  }

  public DateTime? DidNotAttendAt
  {
    get;
    private set;
  }

  private Appointment()
  {
  }

  public Appointment(
      string appointmentReference,
      Guid patientId,
      Guid referralId,
      AppointmentType appointmentType,
      DateTime scheduledStart,
      DateTime scheduledEnd,
      string location)
  {
    if (string.IsNullOrWhiteSpace(
        appointmentReference))
    {
      throw new ArgumentException(
          "Appointment reference is required.");
    }

    if (appointmentReference.Length > 30)
    {
      throw new ArgumentException(
          "Appointment reference cannot exceed 30 characters.");
    }

    if (patientId == Guid.Empty)
    {
      throw new ArgumentException(
          "Patient ID is required.");
    }

    if (referralId == Guid.Empty)
    {
      throw new ArgumentException(
          "Referral ID is required.");
    }

    if (!Enum.IsDefined(
        appointmentType))
    {
      throw new ArgumentException(
          "Appointment type is invalid.");
    }

    if (scheduledEnd <= scheduledStart)
    {
      throw new ArgumentException(
          "Scheduled end must be after scheduled start.");
    }

    if (string.IsNullOrWhiteSpace(
        location))
    {
      throw new ArgumentException(
          "Appointment location is required.");
    }

    if (location.Length > 200)
    {
      throw new ArgumentException(
          "Appointment location cannot exceed 200 characters.");
    }

    Id = Guid.NewGuid();

    AppointmentReference =
        appointmentReference.Trim();

    PatientId =
        patientId;

    ReferralId =
        referralId;

    AppointmentType =
        appointmentType;

    ScheduledStart =
        scheduledStart;

    ScheduledEnd =
        scheduledEnd;

    Location =
        location.Trim();

    Status =
        AppointmentStatus.Scheduled;

    CreatedAt =
        DateTime.UtcNow;
  }

  public void CheckIn()
  {
    if (Status != AppointmentStatus.Scheduled)
    {
      throw new InvalidOperationException(
          $"Cannot check in an appointment from status '{Status}'.");
    }

    var now = DateTime.UtcNow;

    Status =
        AppointmentStatus.CheckedIn;

    CheckedInAt =
        now;

    UpdatedAt =
        now;
  }

  public void Start()
  {
    if (Status != AppointmentStatus.CheckedIn)
    {
      throw new InvalidOperationException(
          $"Cannot start an appointment from status '{Status}'.");
    }

    var now =
        DateTime.UtcNow;

    Status =
        AppointmentStatus.InProgress;

    StartedAt =
        now;

    UpdatedAt =
        now;
  }

  public void Complete()
  {
    if (Status != AppointmentStatus.InProgress)
    {
      throw new InvalidOperationException(
          $"Cannot complete an appointment from status '{Status}'.");
    }

    var now =
        DateTime.UtcNow;

    Status =
        AppointmentStatus.Completed;

    CompletedAt =
        now;

    UpdatedAt =
        now;
  }

  public void Cancel()
  {
    if (Status != AppointmentStatus.Scheduled
        && Status != AppointmentStatus.CheckedIn)
    {
      throw new InvalidOperationException(
          $"Cannot cancel an appointment from status '{Status}'.");
    }

    var now =
        DateTime.UtcNow;

    Status =
        AppointmentStatus.Cancelled;

    CancelledAt =
        now;

    UpdatedAt =
        now;
  }

  public void MarkDidNotAttend()
  {
    if (Status != AppointmentStatus.Scheduled)
    {
      throw new InvalidOperationException(
          $"Cannot mark an appointment as did not attend from status '{Status}'.");
    }

    var now =
        DateTime.UtcNow;

    Status =
        AppointmentStatus.DidNotAttend;

    DidNotAttendAt =
        now;

    UpdatedAt =
        now;
  }
}