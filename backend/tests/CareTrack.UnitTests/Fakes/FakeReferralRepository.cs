using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.Fakes;

public sealed class FakeReferralRepository
    : IReferralRepository
{
  private readonly List<Referral>
      _referrals = [];
  public int SaveChangesCallCount { get; private set; }
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

  public Task<Referral?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    var referral =
        _referrals.SingleOrDefault(
            referral => referral.Id == id);

    return Task.FromResult(referral);
  }

  public Task<IReadOnlyList<ReferralHistoryEntry>>
    GetHistoryAsync(
        Guid referralId,
        CancellationToken cancellationToken = default)
  {
    var referral =
        _referrals.FirstOrDefault(
            referral =>
                referral.Id == referralId);

    IReadOnlyList<ReferralHistoryEntry> result =
        referral?.History
            .OrderBy(
                history =>
                    history.OccurredAt)
            .ToList()
        ?? [];

    return Task.FromResult(
        result);
  }
  public Task SaveChangesAsync(
    CancellationToken cancellationToken = default)
  {
    SaveChangesCallCount++;
    return Task.CompletedTask;
  }

  public IReadOnlyList<Referral> Referrals =>
      _referrals;
}