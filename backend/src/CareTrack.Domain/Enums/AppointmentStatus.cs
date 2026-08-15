namespace CareTrack.Domain.Enums;

public enum AppointmentStatus
{
  Scheduled = 0,
  CheckedIn = 1,
  InProgress = 2,
  Completed = 3,
  Cancelled = 4,
  DidNotAttend = 5
}