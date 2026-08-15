using CareTrack.Application.Common.Models;
using CareTrack.Application.Referrals.SearchReferrals;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

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


  Task<PagedResult<Referral>> SearchAsync(
    ReferralStatus? status,
    ReferralPriority? priority,
    Guid? patientId,
    string? assignedTo,
    DateOnly? createdFrom,
    DateOnly? createdTo,
    int page,
    int pageSize,
    string sortBy,
    string sortDirection,
    CancellationToken cancellationToken = default);

  Task<IReadOnlyList<ReferralHistoryEntry>> GetHistoryAsync(Guid referralId, CancellationToken cancellationToken = default);
  Task SaveChangesAsync(
    CancellationToken cancellationToken = default);

}