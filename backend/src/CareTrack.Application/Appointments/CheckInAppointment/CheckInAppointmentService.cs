using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Appointments.CheckInAppointment;

public class CheckInAppointmentService
{
  private readonly IAppointmentRepository
      _appointmentRepository;

  private readonly ILogger<CheckInAppointmentService>
      _logger;

  public CheckInAppointmentService(
      IAppointmentRepository appointmentRepository,
      ILogger<CheckInAppointmentService> logger)
  {
    _appointmentRepository = appointmentRepository;

    _logger = logger;
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
      appointment.CheckIn();
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
        "Appointment {AppointmentId} checked in",
        appointment.Id);

    return appointment;
  }
}