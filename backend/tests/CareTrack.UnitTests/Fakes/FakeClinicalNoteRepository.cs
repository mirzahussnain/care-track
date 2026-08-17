
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.Fakes;

public sealed class FakeClinicalNoteRepository
    : IClinicalNoteRepository
{
  public List<ClinicalNote> Notes { get; } =
      [];

  public Task<ClinicalNote?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
        Notes.FirstOrDefault(
            note => note.Id == id));
  }

  public Task<IReadOnlyList<ClinicalNote>>
      GetByAppointmentIdAsync(
          Guid appointmentId,
          CancellationToken cancellationToken = default)
  {
    IReadOnlyList<ClinicalNote> result =
        Notes
            .Where(
                note =>
                    note.AppointmentId ==
                    appointmentId)
            .OrderBy(
                note =>
                    note.CreatedAt)
            .ThenBy(
                note =>
                    note.Id)
            .ToList();

    return Task.FromResult(
        result);
  }

  public Task AddAsync(
      ClinicalNote note,
      CancellationToken cancellationToken = default)
  {
    Notes.Add(
        note);

    return Task.CompletedTask;
  }

  public Task SaveChangesAsync(
      CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }
}