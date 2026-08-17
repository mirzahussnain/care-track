using CareTrack.Application.ClinicalNotes.Common;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.ClinicalNotes.UpdateClinicalNote;

public sealed class UpdateClinicalNoteService
{
  private readonly IClinicalNoteRepository
      _clinicalNoteRepository;

  private readonly ILogger<UpdateClinicalNoteService>
      _logger;

  public UpdateClinicalNoteService(
      IClinicalNoteRepository clinicalNoteRepository,
      ILogger<UpdateClinicalNoteService> logger)
  {
    _clinicalNoteRepository =
        clinicalNoteRepository;

    _logger =
        logger;
  }

  public async Task<ClinicalNoteResult> ExecuteAsync(
      UpdateClinicalNoteCommand command,
      CancellationToken cancellationToken = default)
  {
    var note =
        await _clinicalNoteRepository.GetByIdAsync(
            command.Id,
            cancellationToken);

    if (note is null)
    {
      throw new NotFoundException(
          $"Clinical note '{command.Id}' was not found.");
    }

    note.UpdateContent(
        command.Content);

    await _clinicalNoteRepository.SaveChangesAsync(
        cancellationToken);

    _logger.LogInformation(
        "Clinical note {ClinicalNoteId} updated",
        note.Id);

    return new ClinicalNoteResult(
        note.Id,
        note.AppointmentId,
        note.Content,
        note.CreatedBy,
        note.CreatedAt,
        note.UpdatedAt);
  }
}