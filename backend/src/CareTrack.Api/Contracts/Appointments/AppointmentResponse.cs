using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Appointments;

public sealed record AppointmentResponse(
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