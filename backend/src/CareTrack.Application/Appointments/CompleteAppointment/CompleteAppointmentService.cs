using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Appointments.CompleteAppointment;

public class CompleteAppointmentService
{
  private readonly IAppointmentRepository
      _appointmentRepository;

  private readonly ILogger<CompleteAppointmentService>
      _logger;

  public CompleteAppointmentService(
      IAppointmentRepository appointmentRepository,
      ILogger<CompleteAppointmentService> logger)
  {
    _appointmentRepository =
        appointmentRepository;

    _logger =
        logger;
  }

  public async Task<Appointment>
      ExecuteAsync(
          Guid appointmentId,
          CancellationToken cancellationToken = default)
  {
    var appointment =
        await _appointmentRepository
            .GetByIdAsync(
                appointmentId,
                cancellationToken);

    if (appointment is null)
    {
      throw new NotFoundException(
          $"Appointment '{appointmentId}' was not found.");
    }

    try
    {
      appointment.Complete();
    }
    catch (InvalidOperationException ex)
    {
      throw new InvalidStateTransitionException(
          ex.Message);
    }

    await _appointmentRepository
        .SaveChangesAsync(
            cancellationToken);

    _logger.LogInformation(
        "Appointment {AppointmentId} completed",
        appointment.Id);

    return appointment;
  }
}