using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Common.Interfaces;

public interface IAppointmentRepository
{
  Task<Appointment?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default);

  Task<Appointment?> GetByReferenceAsync(
      string appointmentReference,
      CancellationToken cancellationToken = default);

  Task<PagedResult<Appointment>> SearchAsync(
    AppointmentSearchCommand query,
    CancellationToken cancellationToken = default);

  Task<bool> HasSchedulingConflictAsync(
    Guid patientId,
    DateTime scheduledStart,
    DateTime scheduledEnd,
    Guid? excludeAppointmentId = null,
    CancellationToken cancellationToken = default);


  Task AddAsync(
      Appointment appointment,
      CancellationToken cancellationToken = default);

  Task SaveChangesAsync(
      CancellationToken cancellationToken = default);
}