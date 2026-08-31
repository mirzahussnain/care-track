using CareTrack.Domain.Enums;

namespace CareTrack.Application.Common.Models;

public sealed record AppointmentSearchItem(
    Guid Id,
    string AppointmentReference,
    Guid PatientId,
    string PatientReference,
    string PatientDisplayName,
    Guid ReferralId,
    string ReferralReference,
    AppointmentType AppointmentType,
    DateTime ScheduledStart,
    DateTime ScheduledEnd,
    string Location,
    AppointmentStatus Status,
    DateTime CreatedAt);
