using CareTrack.Application.ClinicalNotes.Common;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;

namespace CareTrack.Application.ClinicalNotes.GetClinicalNotesByAppointment;

public sealed class GetClinicalNotesByAppointmentService
{
  private readonly IClinicalNoteRepository
      _clinicalNoteRepository;

  private readonly IAppointmentRepository
      _appointmentRepository;

  public GetClinicalNotesByAppointmentService(
      IClinicalNoteRepository clinicalNoteRepository,
      IAppointmentRepository appointmentRepository)
  {
    _clinicalNoteRepository =
        clinicalNoteRepository;

    _appointmentRepository =
        appointmentRepository;
  }

  public async Task<IReadOnlyList<ClinicalNoteResult>>
      ExecuteAsync(
          Guid appointmentId,
          CancellationToken cancellationToken = default)
  {
    var appointment =
        await _appointmentRepository.GetByIdAsync(
            appointmentId,
            cancellationToken);

    if (appointment is null)
    {
      throw new NotFoundException(
          $"Appointment '{appointmentId}' was not found.");
    }

    var notes =
        await _clinicalNoteRepository
            .GetByAppointmentIdAsync(
                appointmentId,
                cancellationToken);

    return notes
        .Select(
            note =>
                new ClinicalNoteResult(
                    note.Id,
                    note.AppointmentId,
                    note.Content,
                    note.CreatedBy,
                    note.CreatedAt,
                    note.UpdatedAt))
        .ToList();
  }
}