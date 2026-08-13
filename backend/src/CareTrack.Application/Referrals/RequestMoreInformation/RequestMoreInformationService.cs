using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.RequestMoreInformation;

public sealed class RequestMoreInformationService
{
    private readonly IReferralRepository _referralRepository;
    private readonly ILogger<RequestMoreInformationService> _logger;

    public RequestMoreInformationService(
        IReferralRepository referralRepository,
        ILogger<RequestMoreInformationService> logger)
    {
        _referralRepository = referralRepository;
        _logger = logger;
    }

    public async Task<Referral> ExecuteAsync(
        RequestMoreInformationCommand command,
        CancellationToken cancellationToken = default)
    {
        var referral =
            await _referralRepository.GetByIdAsync(
                command.ReferralId,
                cancellationToken);

        if (referral is null)
        {
            _logger.LogWarning(
                "Referral {ReferralId} was not found when requesting more information",
                command.ReferralId);

            throw new NotFoundException(
                $"Referral with id '{command.ReferralId}' was not found.");
        }

        try
        {
            referral.RequestMoreInformation();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidStateTransitionException(
                exception.Message);
        }

        await _referralRepository.SaveChangesAsync(
            cancellationToken);

        _logger.LogInformation(
            "More information requested for referral {ReferralId}",
            referral.Id);

        return referral;
    }
}
