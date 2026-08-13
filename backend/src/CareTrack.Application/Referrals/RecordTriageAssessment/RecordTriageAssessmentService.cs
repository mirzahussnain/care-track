using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.RecordTriageAssessment;

public sealed class RecordTriageAssessmentService
{
  private readonly IReferralRepository
      _referralRepository;

  private readonly ILogger<RecordTriageAssessmentService>
      _logger;

  public RecordTriageAssessmentService(
      IReferralRepository referralRepository,
      ILogger<RecordTriageAssessmentService> logger)
  {
    _referralRepository =
        referralRepository;

    _logger =
        logger;
  }

  public async Task<Referral> ExecuteAsync(
      RecordTriageAssessmentCommand command,
      CancellationToken cancellationToken = default)
  {
    var referral =
        await _referralRepository.GetByIdAsync(
            command.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      _logger.LogWarning(
          "Referral {ReferralId} was not found during triage assessment",
          command.ReferralId);

      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }

    try
    {
      referral.RecordTriageAssessment(
          command.Priority,
          command.Note);
    }
    catch (InvalidOperationException exception)
    {
      throw new InvalidStateTransitionException(
          exception.Message);
    }

    await _referralRepository.SaveChangesAsync(
        cancellationToken);

    _logger.LogInformation(
        "Triage assessment recorded for referral {ReferralId} with priority {Priority}",
        referral.Id,
        referral.Priority);

    return referral;
  }
}