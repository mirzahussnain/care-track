using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.RejectReferral;

public sealed class RejectReferralService
{
    private readonly IReferralRepository _referralRepository;
    private readonly ILogger<RejectReferralService> _logger;

    public RejectReferralService(
        IReferralRepository referralRepository,
        ILogger<RejectReferralService> logger)
    {
        _referralRepository = referralRepository;
        _logger = logger;
    }

    public async Task<Referral> ExecuteAsync(
        RejectReferralCommand command,
        CancellationToken cancellationToken = default)
    {
        var referral =
            await _referralRepository.GetByIdAsync(
                command.ReferralId,
                cancellationToken);

        if (referral is null)
        {
            _logger.LogWarning(
                "Referral {ReferralId} was not found during rejection",
                command.ReferralId);

            throw new NotFoundException(
                $"Referral with id '{command.ReferralId}' was not found.");
        }

        try
        {
            referral.Reject();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidStateTransitionException(
                exception.Message);
        }

        await _referralRepository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Referral {ReferralId} rejected successfully",
            referral.Id);

        return referral;
    }
}
