using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;

using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

using Microsoft.Data.SqlClient;
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

    try
    {
      await _dbContext.SaveChangesAsync(
          cancellationToken);
    }
    catch (DbUpdateException exception)
        when (IsDuplicateReferralReference(exception))
    {
      throw new ConflictException(
          $"A referral with reference '{referral.ReferralReference}' already exists.");
    }
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

  public async Task<PagedResult<Referral>> SearchAsync(
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
         CancellationToken cancellationToken = default)
  {
    IQueryable<Referral> query = _dbContext
        .Referrals
        .AsNoTracking();

    // Filter by current referral status.
    if (status.HasValue)
    {
      query = query.Where(
          referral =>
              referral.Status == status.Value);
    }

    // Filter by current referral priority.
    if (priority.HasValue)
    {
      query = query.Where(
          referral =>
              referral.Priority == priority.Value);
    }

    // Return referrals belonging to a specific patient.
    if (patientId.HasValue)
    {
      query = query.Where(
          referral =>
              referral.PatientId == patientId.Value);
    }

    // Assignment filtering uses the CURRENT assignment.
    // Historical assignments remain in ReferralHistoryEntries.
    if (!string.IsNullOrWhiteSpace(assignedTo))
    {
      var trimmedAssignedTo = assignedTo.Trim();

      query = query.Where(
          referral =>
              referral.AssignedTo == trimmedAssignedTo);
    }

    // Filter referrals created from a specified timestamp.
    if (createdFrom.HasValue)
    {
      var startDate = createdFrom.Value.ToDateTime(
          TimeOnly.MinValue,
          DateTimeKind.Utc);

      query = query.Where(referral => referral.CreatedAt >= startDate);

    }
    // Filter referrals created up to a specified timestamp.
    if (createdTo.HasValue)
    {
      var endDateExclusive = createdTo.Value
   .AddDays(1)
   .ToDateTime(
       TimeOnly.MinValue,
       DateTimeKind.Utc);
      query = query.Where(
          referral =>
              referral.CreatedAt < endDateExclusive);
    }

    // Count matching referrals BEFORE pagination.
    var totalCount = await query.CountAsync(
        cancellationToken);

    // Apply deterministic sorting before Skip/Take.
    query = ApplySorting(
        query,
        sortBy,
        sortDirection);

    var skip =
        (page - 1) * pageSize;

    var items = await query
        .Skip(skip)
        .Take(pageSize)
        .ToListAsync(
            cancellationToken);

    var totalPages = (totalCount + pageSize - 1) / pageSize;

    return new PagedResult<Referral>(
        items,
        page,
        pageSize,
        totalCount,
        totalPages);

  }

  private static IQueryable<Referral> ApplySorting(
      IQueryable<Referral> query,
      string sortBy,
      string sortDirection)
  {
    var ascending = sortDirection.Equals(
        "asc",
        StringComparison.OrdinalIgnoreCase);

    return sortBy.ToLowerInvariant() switch
    {
      "createdat" =>
          ascending
              ? query
                  .OrderBy(referral => referral.CreatedAt)
                  .ThenBy(referral => referral.ReferralReference)
              : query
                  .OrderByDescending(referral => referral.CreatedAt)
                  .ThenBy(referral => referral.ReferralReference),

      "updatedat" =>
          ascending
              ? query
                  .OrderBy(referral => referral.UpdatedAt)
                  .ThenBy(referral => referral.ReferralReference)
              : query
                  .OrderByDescending(referral => referral.UpdatedAt)
                  .ThenBy(referral => referral.ReferralReference),

      "priority" =>
          ascending
              ? query
                  .OrderBy(referral => referral.Priority)
                  .ThenBy(referral => referral.ReferralReference)
              : query
                  .OrderByDescending(referral => referral.Priority)
                  .ThenBy(referral => referral.ReferralReference),

      "status" =>
          ascending
              ? query
                  .OrderBy(referral => referral.Status)
                  .ThenBy(referral => referral.ReferralReference)
              : query
                  .OrderByDescending(referral => referral.Status)
                  .ThenBy(referral => referral.ReferralReference),

      "referralreference" =>
          ascending
              ? query
                  .OrderBy(referral => referral.ReferralReference)
                  .ThenBy(referral => referral.Id)

              : query
                  .OrderByDescending(referral => referral.ReferralReference)
                  .ThenBy(referral => referral.Id)
,

      _ => throw new ArgumentException(
          $"Unsupported referral sort field '{sortBy}'.")
    };
  }

  private static bool IsDuplicateReferralReference(
      DbUpdateException exception)
  {
    return exception.InnerException is SqlException sqlException
        && (sqlException.Number == 2601 || sqlException.Number == 2627)
        && sqlException.Message.Contains(
            "IX_Referrals_ReferralReference",
            StringComparison.OrdinalIgnoreCase);
  }
}