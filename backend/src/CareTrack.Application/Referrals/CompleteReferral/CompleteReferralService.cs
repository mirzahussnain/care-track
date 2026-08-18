using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.CompleteReferral;

public sealed class CompleteReferralService
{
    private readonly IReferralRepository
        _referralRepository;

    private readonly IAppointmentRepository
        _appointmentRepository;

    private readonly ILogger<CompleteReferralService>
        _logger;

    public CompleteReferralService(
        IReferralRepository referralRepository,
        IAppointmentRepository appointmentRepository,
        ILogger<CompleteReferralService> logger)
    {
        _referralRepository =
            referralRepository;

        _appointmentRepository =
            appointmentRepository;

        _logger =
            logger;
    }

    public async Task ExecuteAsync(
        CompleteReferralCommand command,
        CancellationToken cancellationToken = default)
    {
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

        var appointments =
            await _appointmentRepository
                .GetByReferralIdAsync(
                    referral.Id,
                    cancellationToken);

        if (!appointments.Any())
        {
            throw new ConflictException(
                "The referral cannot be completed because it has no appointments.");
        }

        if (!appointments.Any(
            appointment =>
                appointment.Status ==
                AppointmentStatus.Completed))
        {
            throw new ConflictException(
                "The referral cannot be completed because no appointment has been completed.");
        }

        var hasActiveAppointments =
            appointments.Any(
                appointment =>
                    appointment.Status is
                        AppointmentStatus.Scheduled
                        or AppointmentStatus.CheckedIn
                        or AppointmentStatus.InProgress);

        if (hasActiveAppointments)
        {
            throw new ConflictException(
                "The referral cannot be completed while active appointments remain.");
        }

        try
        {
            referral.Complete();
        }
        catch (InvalidOperationException ex)
        {
            throw new InvalidStateTransitionException(ex.Message);
        }

        await _referralRepository
            .SaveChangesAsync(
                cancellationToken);

        _logger.LogInformation(
            "Referral {ReferralId} completed",
            referral.Id);
    }
}