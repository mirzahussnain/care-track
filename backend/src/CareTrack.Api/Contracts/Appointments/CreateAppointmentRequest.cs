using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Appointments;

public sealed record CreateAppointmentRequest(
    string AppointmentReference,
    Guid PatientId,
    Guid ReferralId,
    AppointmentType AppointmentType,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Location);