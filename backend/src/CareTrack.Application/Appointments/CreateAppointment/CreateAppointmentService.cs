using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Appointments.CreateAppointment;

public class CreateAppointmentService
{
  private readonly IAppointmentRepository _appointmentRepository;
  private readonly IPatientRepository _patientRepository;
  private readonly IReferralRepository _referralRepository;
  private readonly ILogger<CreateAppointmentService> _logger;
  private readonly IApplicationTransaction _applicationTransaction;
  public CreateAppointmentService(
      IAppointmentRepository appointmentRepository,
      IPatientRepository patientRepository,
      IReferralRepository referralRepository,
      IApplicationTransaction applicationTransaction,
      ILogger<CreateAppointmentService> logger
      )
  {
    _appointmentRepository =
        appointmentRepository;

    _patientRepository =
        patientRepository;

    _referralRepository =
        referralRepository;

    _logger =
        logger;

    _applicationTransaction =
        applicationTransaction;
  }

  public async Task<AppointmentDetailsResult>
      ExecuteAsync(
          CreateAppointmentCommand command,
          CancellationToken cancellationToken = default)
  {
    var existingAppointment =
        await _appointmentRepository
            .GetByReferenceAsync(
                command.AppointmentReference,
                cancellationToken);

    if (existingAppointment is not null)
    {
      throw new ConflictException(
          $"Appointment reference '{command.AppointmentReference}' already exists.");
    }

    var patient =
        await _patientRepository
            .GetByIdAsync(
                command.PatientId,
                cancellationToken);

    if (patient is null)
    {
      throw new NotFoundException(
          $"Patient '{command.PatientId}' was not found.");
    }

    var referral =
        await _referralRepository
            .GetByIdAsync(
                command.ReferralId,
                cancellationToken);

    if (referral is null)
    {
      throw new NotFoundException(
          $"Referral '{command.ReferralId}' was not found.");
    }

    if (referral.PatientId != patient.Id)
    {
      throw new ArgumentException(
          "The referral does not belong to the specified patient.");
    }

    if (!referral.CanScheduleAppointment())
{
    throw new ConflictException(
        $"Referral '{referral.Id}' cannot be scheduled while in status '{referral.Status}'.");
}

    var appointment =
        new Appointment(
            command.AppointmentReference,
            command.PatientId,
            command.ReferralId,
            command.AppointmentType,
            command.ScheduledStart,
            command.ScheduledEnd,
            command.Location);

    var hasSchedulingConflict =
    await _appointmentRepository
        .HasSchedulingConflictAsync(
            appointment.PatientId,
            appointment.ScheduledStart,
            appointment.ScheduledEnd,
            cancellationToken: cancellationToken);

    if (hasSchedulingConflict)
    {
      throw new ConflictException(
          "The patient already has an overlapping appointment.");
    }

    await _applicationTransaction.ExecuteAsync(
    async ct =>
    {
        await _appointmentRepository.AddAsync(
            appointment,
            ct);

        if (referral.Status == ReferralStatus.Assigned)
        {
            referral.Schedule();

            await _referralRepository.SaveChangesAsync(
                ct);
        }
    },
    cancellationToken);

    _logger.LogInformation(
        "Appointment {AppointmentId} created successfully",
        appointment.Id);

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
    appointment.DidNotAttendAt);
  }
}