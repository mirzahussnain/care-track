using CareTrack.Application.ClinicalNotes.Common;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.ClinicalNotes.CreateClinicalNote;

public sealed class CreateClinicalNoteService
{
  private readonly IClinicalNoteRepository
      _clinicalNoteRepository;

  private readonly IAppointmentRepository
      _appointmentRepository;

  private readonly ILogger<CreateClinicalNoteService>
      _logger;

  public CreateClinicalNoteService(
      IClinicalNoteRepository clinicalNoteRepository,
      IAppointmentRepository appointmentRepository,
      ILogger<CreateClinicalNoteService> logger)
  {
    _clinicalNoteRepository =
        clinicalNoteRepository;

    _appointmentRepository =
        appointmentRepository;

    _logger =
        logger;
  }

  public async Task<ClinicalNoteResult>
      ExecuteAsync(
          CreateClinicalNoteCommand command,
          CancellationToken cancellationToken = default)
  {
    var note =
        new ClinicalNote(
            command.AppointmentId,
            command.Content,
            command.CreatedBy);

    var appointment =
        await _appointmentRepository
            .GetByIdAsync(
                note.AppointmentId,
                cancellationToken);

    if (appointment is null)
    {
      throw new NotFoundException(
          $"Appointment '{note.AppointmentId}' was not found.");
    }

    await _clinicalNoteRepository
        .AddAsync(
            note,
            cancellationToken);

    _logger.LogInformation(
        "Clinical note {ClinicalNoteId} created for appointment {AppointmentId}",
        note.Id,
        note.AppointmentId);

    return new ClinicalNoteResult(
        note.Id,
        note.AppointmentId,
        note.Content,
        note.CreatedBy,
        note.CreatedAt,
        note.UpdatedAt);
  }
}

