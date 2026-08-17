using CareTrack.Domain.Enums;

namespace CareTrack.IntegrationTests.Contracts.Appointments;

public sealed record AppointmentSearchItemResponse(
    Guid Id,
    string AppointmentReference,
    Guid PatientId,
    Guid ReferralId,
    AppointmentType AppointmentType,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Location,
    AppointmentStatus Status,
    DateTime CreatedAt);