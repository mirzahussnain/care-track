using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Appointments.StartAppointment;

public class StartAppointmentService
{
  private readonly IAppointmentRepository
      _appointmentRepository;

  private readonly ILogger<StartAppointmentService>
      _logger;

  public StartAppointmentService(
      IAppointmentRepository appointmentRepository,
      ILogger<StartAppointmentService> logger)
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
      appointment.Start();
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
        "Appointment {AppointmentId} started",
        appointment.Id);

    return appointment;
  }
}