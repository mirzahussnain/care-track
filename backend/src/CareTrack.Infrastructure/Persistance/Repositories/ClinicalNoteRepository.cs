using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using CareTrack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance.Repositories;

public sealed class ClinicalNoteRepository
    : IClinicalNoteRepository
{
  private readonly CareTrackDbContext
      _dbContext;

  public ClinicalNoteRepository(
      CareTrackDbContext dbContext)
  {
    _dbContext =
        dbContext;
  }

  public Task<ClinicalNote?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    return _dbContext.ClinicalNotes
        .FirstOrDefaultAsync(
            note => note.Id == id,
            cancellationToken);
  }

  public async Task<IReadOnlyList<ClinicalNote>>
      GetByAppointmentIdAsync(
          Guid appointmentId,
          CancellationToken cancellationToken = default)
  {
    return await _dbContext.ClinicalNotes
        .AsNoTracking()
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
        .ToListAsync(
            cancellationToken);
  }

  public async Task AddAsync(
      ClinicalNote note,
      CancellationToken cancellationToken = default)
  {
    await _dbContext.ClinicalNotes
        .AddAsync(
            note,
            cancellationToken);

    await _dbContext.SaveChangesAsync(
        cancellationToken);
  }

  public Task SaveChangesAsync(
      CancellationToken cancellationToken = default)
  {
    return _dbContext.SaveChangesAsync(
        cancellationToken);
  }
}