using CareTrack.Application.ClinicalNotes.Common;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;

namespace CareTrack.Application.ClinicalNotes.GetClinicalNoteById;

public sealed class GetClinicalNoteByIdService
{
  private readonly IClinicalNoteRepository
      _clinicalNoteRepository;

  public GetClinicalNoteByIdService(
      IClinicalNoteRepository clinicalNoteRepository)
  {
    _clinicalNoteRepository =
        clinicalNoteRepository;
  }

  public async Task<ClinicalNoteResult> ExecuteAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    var note =
        await _clinicalNoteRepository.GetByIdAsync(
            id,
            cancellationToken);

    if (note is null)
    {
      throw new NotFoundException(
          $"Clinical note '{id}' was not found.");
    }

    return new ClinicalNoteResult(
        note.Id,
        note.AppointmentId,
        note.Content,
        note.CreatedBy,
        note.CreatedAt,
        note.UpdatedAt);
  }
}