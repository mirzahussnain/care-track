using CareTrack.Domain.Enums;
namespace CareTrack.Application.Appointments.SearchAppointments;

public sealed record AppointmentSearchCommand(
    Guid? PatientId,
    Guid? ReferralId,
    AppointmentStatus? Status,
    AppointmentType? AppointmentType,
    string? Location,
    DateTime? ScheduledFrom,
    DateTime? ScheduledTo,
    int Page = 1,
    int PageSize = 20,
    string SortBy = "scheduledStart",
    string SortDirection = "asc");