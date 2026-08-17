using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Appointments;

public sealed class SearchAppointmentsRequest
{
  public Guid? PatientId { get; init; }

  public Guid? ReferralId { get; init; }

  public AppointmentStatus? Status { get; init; }

  public AppointmentType? AppointmentType { get; init; }

  public string? Location { get; init; }

  public DateTime? ScheduledFrom { get; init; }

  public DateTime? ScheduledTo { get; init; }

  public int Page { get; init; } = 1;

  public int PageSize { get; init; } = 20;

  public string SortBy { get; init; } =
      "scheduledStart";

  public string SortDirection { get; init; } =
      "asc";
}