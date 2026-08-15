using CareTrack.Domain.Enums;

namespace CareTrack.Application.Common.Models;

public sealed record CreateAppointmentResult(
    Guid Id,
    string AppointmentReference,
    Guid PatientId,
    Guid ReferralId,
    AppointmentType AppointmentType,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Location,
    AppointmentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);