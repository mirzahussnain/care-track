using CareTrack.Domain.Entities;

namespace CareTrack.Application.Common.Interfaces;

public interface IClinicalNoteRepository
{
  Task<ClinicalNote?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default);

  Task<IReadOnlyList<ClinicalNote>> GetByAppointmentIdAsync(
      Guid appointmentId,
      CancellationToken cancellationToken = default);

  Task AddAsync(
      ClinicalNote note,
      CancellationToken cancellationToken = default);

  Task SaveChangesAsync(
      CancellationToken cancellationToken = default);
}