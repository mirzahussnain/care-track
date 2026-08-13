using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.Fakes;

public sealed class FakeReferralRepository
    : IReferralRepository
{
  private readonly List<Referral>
      _referrals = [];

  public Task<Referral?>
      GetByReferenceAsync(
          string referralReference,
          CancellationToken cancellationToken =
              default)
  {
    var referral =
        _referrals.SingleOrDefault(
            referral =>
                referral.ReferralReference ==
                referralReference);

    return Task.FromResult(referral);
  }

  public Task AddAsync(
      Referral referral,
      CancellationToken cancellationToken =
          default)
  {
    _referrals.Add(referral);

    return Task.CompletedTask;
  }

  public IReadOnlyList<Referral> Referrals =>
      _referrals;
}