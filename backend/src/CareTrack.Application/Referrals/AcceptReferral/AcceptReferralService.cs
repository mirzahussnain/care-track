using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.AcceptReferral;

public sealed class AcceptReferralService
{
  private readonly IReferralRepository _referralRepository;
  private readonly ILogger<AcceptReferralService> _logger;

  public AcceptReferralService(IReferralRepository referralRepository, ILogger<AcceptReferralService> logger)
  {
    _referralRepository = referralRepository;
    _logger = logger;
  }

  public async Task<Referral> ExecuteAsync(AcceptReferralCommand command, CancellationToken cancellationToken = default)
  {
    var referral = await _referralRepository.GetByIdAsync(command.ReferralId, cancellationToken);
    if (referral is null)
    {
      _logger.LogWarning(
     "Referral {ReferralId} was not found during acceptance",
     command.ReferralId);

      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }
    try
    {
      referral.Accept();
    }
    catch (InvalidOperationException exception)
    {
      throw new InvalidStateTransitionException(
          exception.Message);
    }
    await _referralRepository.SaveChangesAsync(
       cancellationToken);

    _logger.LogInformation(
       "Referral {ReferralId} has been accepted",
       referral.Id);

    return referral;

  }

}