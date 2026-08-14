using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.AssignReferral;

public sealed class AssignReferralService
{
  private readonly IReferralRepository _referralRepository;

  private readonly ILogger<AssignReferralService> _logger;

  public AssignReferralService(
      IReferralRepository referralRepository,
      ILogger<AssignReferralService> logger)
  {
    _referralRepository = referralRepository;

    _logger = logger;
  }

  public async Task<Referral> ExecuteAsync(
      AssignReferralCommand command,
      CancellationToken cancellationToken = default)
  {
    var referral =
        await _referralRepository.GetByIdAsync(
            command.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      _logger.LogWarning(
          "Referral {ReferralId} was not found during assignment",
          command.ReferralId);

      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }

    try
    {
      referral.Assign(command.AssignedTo);
    }
    catch (InvalidOperationException exception)
    {
      throw new InvalidStateTransitionException(
          exception.Message);
    }

    await _referralRepository.SaveChangesAsync(
        cancellationToken);

    _logger.LogInformation(
        "Referral {ReferralId} assigned successfully",
        referral.Id);

    return referral;
  }
}