using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.SubmitReferral;

public sealed class SubmitReferralService
{
  private readonly IReferralRepository _referralRepository;
  private readonly ILogger<SubmitReferralService> _logger;

  public SubmitReferralService(
      IReferralRepository referralRepository,
      ILogger<SubmitReferralService> logger)
  {
    _referralRepository = referralRepository;
    _logger = logger;
  }

  public async Task<Referral> ExecuteAsync(
      SubmitReferralCommand command,
      CancellationToken cancellationToken = default)
  {
    var referral =
        await _referralRepository.GetByIdAsync(
            command.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      _logger.LogWarning(
          "Referral {ReferralId} was not found during submission",
          command.ReferralId);

      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }

    try
    {
      referral.Submit();
    }
    catch (InvalidOperationException exception)
    {
      throw new InvalidStateTransitionException(
          exception.Message);
    }

    await _referralRepository.SaveChangesAsync(
        cancellationToken);

    _logger.LogInformation(
        "Referral {ReferralId} submitted successfully",
        referral.Id);

    return referral;
  }
}