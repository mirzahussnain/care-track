using CareTrack.Domain.Enums;

namespace CareTrack.Application.Appointments.CreateAppointment;

public sealed record CreateAppointmentCommand(
    string AppointmentReference,
    Guid PatientId,
    Guid ReferralId,
    AppointmentType AppointmentType,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Location);