using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using CareTrack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance.Repositories;

public sealed class ReferralRepository
    : IReferralRepository
{
  private readonly CareTrackDbContext _dbContext;

  public ReferralRepository(
      CareTrackDbContext dbContext)
  {
    _dbContext = dbContext;
  }
  public Task SaveChangesAsync(
    CancellationToken cancellationToken = default)
  {
    foreach (var entry in _dbContext.ChangeTracker.Entries())
    {
      Console.WriteLine(
          $"{entry.Entity.GetType().Name} - {entry.State}");
    }
    return _dbContext.SaveChangesAsync(
        cancellationToken);
  }

  public Task<Referral?>
      GetByReferenceAsync(
          string referralReference,
          CancellationToken cancellationToken =
              default)
  {
    return _dbContext.Referrals
        .AsNoTracking()
        .SingleOrDefaultAsync(
            referral =>
                referral.ReferralReference ==
                referralReference,
            cancellationToken);
  }

  public async Task AddAsync(
      Referral referral,
      CancellationToken cancellationToken =
          default)
  {
    await _dbContext.Referrals.AddAsync(
        referral,
        cancellationToken);

    await _dbContext.SaveChangesAsync(
        cancellationToken);
  }

  public Task<Referral?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return _dbContext.Referrals
        .SingleOrDefaultAsync(
            referral => referral.Id == id,
            cancellationToken);
  }

  public async Task<IReadOnlyList<ReferralHistoryEntry>>
    GetHistoryAsync(
        Guid referralId,
        CancellationToken cancellationToken = default)
  {
    return await _dbContext
        .ReferralHistoryEntries
        .AsNoTracking()
        .Where(history =>
            history.ReferralId == referralId)
        .OrderBy(history =>
            history.OccurredAt)
        .ThenBy(history =>
            history.Id)
        .ToListAsync(
            cancellationToken);
  }


}