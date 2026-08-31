using CareTrack.Domain.Enums;

namespace CareTrack.Infrastructure.Persistance.ReadModels;

/// <summary>
/// Read-only row mapped to dbo.vw_AppointmentOperationalList.
/// This type is infrastructure-only and is never used for transactional writes.
/// </summary>
public sealed class AppointmentOperationalListReadModel
{
  public Guid Id { get; init; }
  public string AppointmentReference { get; init; } = null!;
  public AppointmentType AppointmentType { get; init; }
  public AppointmentStatus Status { get; init; }
  public DateTime ScheduledStart { get; init; }
  public DateTime ScheduledEnd { get; init; }
  public string Location { get; init; } = null!;
  public DateTime CreatedAt { get; init; }
  public Guid PatientId { get; init; }
  public string PatientReference { get; init; } = null!;
  public string PatientDisplayName { get; init; } = null!;
  public Guid ReferralId { get; init; }
  public string ReferralReference { get; init; } = null!;
}
