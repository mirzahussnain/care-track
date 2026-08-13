using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.ResubmitReferral;

public sealed class ResubmitReferralService
{
    private readonly IReferralRepository _referralRepository;
    private readonly ILogger<ResubmitReferralService> _logger;

    public ResubmitReferralService(
        IReferralRepository referralRepository,
        ILogger<ResubmitReferralService> logger)
    {
        _referralRepository = referralRepository;
        _logger = logger;
    }

    public async Task<Referral> ExecuteAsync(
        ResubmitReferralCommand command,
        CancellationToken cancellationToken = default)
    {
        var referral =
            await _referralRepository.GetByIdAsync(
                command.ReferralId,
                cancellationToken);

        if (referral is null)
        {
            _logger.LogWarning(
                "Referral {ReferralId} was not found during resubmission",
                command.ReferralId);

            throw new NotFoundException(
                $"Referral with id '{command.ReferralId}' was not found.");
        }

        try
        {
            referral.Resubmit();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidStateTransitionException(
                exception.Message);
        }

        await _referralRepository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "Referral {ReferralId} resubmitted successfully",
            referral.Id);

        return referral;
    }
}
