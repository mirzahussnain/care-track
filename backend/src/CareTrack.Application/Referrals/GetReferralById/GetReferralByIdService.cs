using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Referrals.GetReferralById;

public sealed class GetReferralByIdService
{
  private readonly IReferralRepository
      _referralRepository;

  public GetReferralByIdService(
      IReferralRepository referralRepository)
  {
    _referralRepository =
        referralRepository;
  }

  public async Task<Referral>
      ExecuteAsync(
          GetReferralByIdCommand query,
          CancellationToken cancellationToken = default)
  {
    var referral =
        await _referralRepository.GetByIdAsync(
            query.ReferralId,
            cancellationToken);

    if (referral is null)
    {
      throw new NotFoundException(
          $"Referral with id '{query.ReferralId}' was not found.");
    }

    return referral;
  }
}