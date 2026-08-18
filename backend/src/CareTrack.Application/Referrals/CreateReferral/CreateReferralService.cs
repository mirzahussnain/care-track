using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Referrals.CreateReferral;

public sealed class CreateReferralService
{
  private readonly IPatientRepository
      _patientRepository;

  private readonly IReferralRepository
      _referralRepository;

  private readonly ILogger<CreateReferralService>
      _logger;

  public CreateReferralService(
      IPatientRepository patientRepository,
      IReferralRepository referralRepository,
      ILogger<CreateReferralService> logger)
  {
    _patientRepository =
        patientRepository;

    _referralRepository =
        referralRepository;

    _logger =
        logger;
  }

  public async Task<Referral> ExecuteAsync(
      CreateReferralCommand command,
      CancellationToken cancellationToken =
          default)
  {
    var referralReference = command.ReferralReference.Trim();

    var patient =
        await _patientRepository.GetByIdAsync(
            command.PatientId,
            cancellationToken);

    if (patient is null)
    {
      _logger.LogWarning(
          "Referral creation rejected because patient {PatientId} was not found",
          command.PatientId);

      throw new NotFoundException(
          $"Patient with id '{command.PatientId}' was not found.");
    }

    var existingReferral =
        await _referralRepository
            .GetByReferenceAsync(
                referralReference,
                cancellationToken);

    if (existingReferral is not null)
    {
      _logger.LogWarning(
          "Referral creation rejected because reference {ReferralReference} already exists",
          referralReference);

      throw new ConflictException(
          $"A referral with reference '{referralReference}' already exists.");
    }

    var referral =
        new Referral(
            referralReference,
            command.PatientId,
            command.Priority,
            command.Reason);

    await _referralRepository.AddAsync(
        referral,
        cancellationToken);

    _logger.LogInformation(
        "Referral {ReferralId} created for patient {PatientId}",
        referral.Id,
        referral.PatientId);

    return referral;
  }
}