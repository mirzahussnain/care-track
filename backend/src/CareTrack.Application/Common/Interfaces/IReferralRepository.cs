using CareTrack.Domain.Entities;

namespace CareTrack.Application.Common.Interfaces;

public interface IReferralRepository
{
  Task<Referral?> GetByReferenceAsync(
      string referralReference,
      CancellationToken cancellationToken = default);

  Task AddAsync(
      Referral referral,
      CancellationToken cancellationToken = default);
}