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
  Task<Referral?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default);

  Task<IReadOnlyList<ReferralHistoryEntry>> GetHistoryAsync(Guid referralId, CancellationToken cancellationToken = default);
  Task SaveChangesAsync(
    CancellationToken cancellationToken = default);

}