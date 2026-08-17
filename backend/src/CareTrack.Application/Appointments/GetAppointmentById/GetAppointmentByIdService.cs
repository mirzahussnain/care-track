using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;

namespace CareTrack.Application.Appointments.GetAppointmentById;

public sealed class GetAppointmentByIdService
{
  private readonly IAppointmentRepository
      _appointmentRepository;

  public GetAppointmentByIdService(
      IAppointmentRepository appointmentRepository)
  {
    _appointmentRepository =
        appointmentRepository;
  }

  public async Task<AppointmentDetailsResult>
      ExecuteAsync(
          Guid id,
          CancellationToken cancellationToken = default)
  {
    var appointment =
        await _appointmentRepository
            .GetByIdAsync(
                id,
                cancellationToken);

    if (appointment is null)
    {
      throw new NotFoundException(
          $"Appointment '{id}' was not found.");
    }

    return new AppointmentDetailsResult(
        appointment.Id,
        appointment.AppointmentReference,
        appointment.PatientId,
        appointment.ReferralId,
        appointment.AppointmentType,
        appointment.ScheduledStart,
        appointment.ScheduledEnd,
        appointment.Location,
        appointment.Status,
        appointment.CreatedAt,
        appointment.UpdatedAt,
        appointment.CheckedInAt,
        appointment.StartedAt,
        appointment.CompletedAt,
        appointment.CancelledAt,
        appointment.DidNotAttendAt
        );
  }
}
