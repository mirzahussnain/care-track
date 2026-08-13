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
}