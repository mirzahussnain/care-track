using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace CareTrack.Application.Referrals.StartTriage;

public sealed class StartTriageService
{
  private readonly IReferralRepository _referralRepository;
  private readonly ILogger<StartTriageService> _logger;

  public StartTriageService(IReferralRepository referralRepository, ILogger<StartTriageService> logger)
  {
    _referralRepository = referralRepository;
    _logger = logger;
  }
  public async Task<Referral> ExecuteAsync(
      StartTriageCommand command,
      CancellationToken cancellationToken = default)
  {
    var referral = await _referralRepository.GetByIdAsync(
            command.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }

    try
    {
      referral.StartTriage();
    }
    catch (InvalidOperationException exception)
    {
      _logger.LogWarning(
     "Referral {ReferralId} could not enter triage because the current state does not allow the transition",
     command.ReferralId);

      throw new InvalidStateTransitionException(
          exception.Message);
    }

    await _referralRepository.SaveChangesAsync(
        cancellationToken);

    _logger.LogInformation(
        "Referral {ReferralId} entered triage",
        referral.Id);

    return referral;
  }

}