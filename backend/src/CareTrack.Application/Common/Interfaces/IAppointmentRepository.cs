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

  Task AddAsync(
      Appointment appointment,
      CancellationToken cancellationToken = default);

  Task SaveChangesAsync(
      CancellationToken cancellationToken = default);
}