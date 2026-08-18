using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Appointments.StartAppointment;

public class StartAppointmentService
{
  private readonly IAppointmentRepository _appointmentRepository;
  private readonly IReferralRepository _referralRepository;
  private readonly IApplicationTransaction _applicationTransaction;

  private readonly ILogger<StartAppointmentService>
      _logger;

  public StartAppointmentService(
      IAppointmentRepository appointmentRepository,
       IReferralRepository referralRepository,
    IApplicationTransaction applicationTransaction,
      ILogger<StartAppointmentService> logger)
  {
    _appointmentRepository =
        appointmentRepository;

    _referralRepository =
      referralRepository;

    _applicationTransaction =
        applicationTransaction;

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
    var referral =
    await _referralRepository.GetByIdAsync(
        appointment.ReferralId,
        cancellationToken);

    if (referral is null)
    {
      throw new NotFoundException(
          $"Referral '{appointment.ReferralId}' was not found.");
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

    var referralStarted = false;

    if (referral.Status ==
    ReferralStatus.Scheduled)
    {
      referral.StartProgress();

      referralStarted =
          true;
    }

    await _applicationTransaction.ExecuteAsync(
    async ct =>
    {
      await _appointmentRepository
          .SaveChangesAsync(ct);

      if (referralStarted)
      {
        await _referralRepository
            .SaveChangesAsync(ct);
      }
    },
    cancellationToken);

    _logger.LogInformation(
        "Appointment {AppointmentId} started",
        appointment.Id);

    return appointment;
  }
}