using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Referrals.GetReferralHistory;

public sealed class GetReferralHistoryService
{
  private readonly IReferralRepository
      _referralRepository;

  public GetReferralHistoryService(
      IReferralRepository referralRepository)
  {
    _referralRepository = referralRepository;
  }

  public async Task<IReadOnlyList<ReferralHistoryEntry>>
      ExecuteAsync(
          GetReferralHistoryCommand command,
          CancellationToken cancellationToken = default)
  {
    var referral =
        await _referralRepository.GetByIdAsync(
            command.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      throw new NotFoundException(
          $"Referral with id '{command.ReferralId}' was not found.");
    }

    return await _referralRepository
        .GetHistoryAsync(
            command.ReferralId,
            cancellationToken);
  }
}