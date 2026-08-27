using System.Data;
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
    Appointment appointment = null!;
    DateTime startedAtMarker = default;

    await _applicationTransaction.ExecuteAsync(
    async ct =>
    {
      appointment =
          await _appointmentRepository
              .GetByIdAsync(
                  appointmentId,
                  ct)
          ?? throw new NotFoundException(
              $"Appointment '{appointmentId}' was not found.");

      var referral =
          await _referralRepository
              .GetByIdAsync(
                  appointment.ReferralId,
                  ct)
          ?? throw new NotFoundException(
              $"Referral '{appointment.ReferralId}' was not found.");

      startedAtMarker = DateTime.UtcNow;

      try
      {
        appointment.Start(
            startedAtMarker);
      }
      catch (InvalidOperationException exception)
      {
        throw new InvalidStateTransitionException(
            exception.Message);
      }

      if (referral.Status ==
          ReferralStatus.Scheduled)
      {
        referral.StartProgress();
      }

      await _appointmentRepository
          .SaveChangesAsync(ct);
    },
    async ct =>
    {
      var persistedAppointment =
          await _appointmentRepository
              .GetByIdAsync(
                  appointmentId,
                  ct);

      return persistedAppointment?.StartedAt ==
          startedAtMarker;
    },
    IsolationLevel.ReadCommitted,
    cancellationToken);

    _logger.LogInformation(
        "Appointment {AppointmentId} started",
        appointment.Id);

    return appointment;
  }
}