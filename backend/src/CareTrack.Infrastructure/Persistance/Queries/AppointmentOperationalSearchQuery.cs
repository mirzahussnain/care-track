using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Models;
using CareTrack.Infrastructure.Persistance.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance.Queries;

/// <summary>
/// Read-only appointment list query. Filtering, ordering, counting, and paging
/// are translated to SQL against the operational view.
/// </summary>
public sealed class AppointmentOperationalSearchQuery : IAppointmentSearchQuery
{
  private readonly CareTrackDbContext _dbContext;

  public AppointmentOperationalSearchQuery(CareTrackDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<PagedResult<AppointmentSearchItem>> SearchAsync(
      AppointmentSearchCommand command,
      CancellationToken cancellationToken = default)
  {
    IQueryable<AppointmentOperationalListReadModel> query =
        _dbContext.AppointmentOperationalList.AsNoTracking();

    if (command.PatientId.HasValue)
      query = query.Where(a => a.PatientId == command.PatientId.Value);

    if (command.ReferralId.HasValue)
      query = query.Where(a => a.ReferralId == command.ReferralId.Value);

    if (command.Status.HasValue)
      query = query.Where(a => a.Status == command.Status.Value);

    if (command.AppointmentType.HasValue)
      query = query.Where(a => a.AppointmentType == command.AppointmentType.Value);

    if (!string.IsNullOrWhiteSpace(command.Location))
    {
      var location = command.Location.Trim();
      query = query.Where(a => a.Location.Contains(location));
    }

    if (command.ScheduledFrom.HasValue && command.ScheduledTo.HasValue)
    {
      var from = command.ScheduledFrom.Value;
      var to = command.ScheduledTo.Value;
      query = query.Where(a => a.ScheduledStart < to && a.ScheduledEnd > from);
    }
    else if (command.ScheduledFrom.HasValue)
    {
      var from = command.ScheduledFrom.Value;
      query = query.Where(a => a.ScheduledEnd > from);
    }
    else if (command.ScheduledTo.HasValue)
    {
      var to = command.ScheduledTo.Value;
      query = query.Where(a => a.ScheduledStart < to);
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var descending = command.SortDirection.Equals(
        "desc", StringComparison.OrdinalIgnoreCase);
    query = ApplyOrdering(query, command.SortBy, descending);

    return await BuildResultAsync(query, command, totalCount, cancellationToken);
  }

  private static async Task<PagedResult<AppointmentSearchItem>> BuildResultAsync(
      IQueryable<AppointmentOperationalListReadModel> query,
      AppointmentSearchCommand command,
      int totalCount,
      CancellationToken cancellationToken)
  {
    var pageQuery = query
        .Skip((command.Page - 1) * command.PageSize)
        .Take(command.PageSize);
    return await ProjectAsync(pageQuery, command, totalCount, cancellationToken);
  }

  private static IQueryable<AppointmentOperationalListReadModel> ApplyOrdering(
      IQueryable<AppointmentOperationalListReadModel> query,
      string sortBy,
      bool descending)
  {
    return sortBy.ToLowerInvariant() switch
    {
      "scheduledend" => descending
          ? query.OrderByDescending(a => a.ScheduledEnd).ThenByDescending(a => a.Id)
          : query.OrderBy(a => a.ScheduledEnd).ThenBy(a => a.Id),
      "createdat" => descending
          ? query.OrderByDescending(a => a.CreatedAt).ThenByDescending(a => a.Id)
          : query.OrderBy(a => a.CreatedAt).ThenBy(a => a.Id),
      _ => ApplyRemainingOrdering(query, sortBy, descending)
    };
  }

  private static IQueryable<AppointmentOperationalListReadModel> ApplyRemainingOrdering(
      IQueryable<AppointmentOperationalListReadModel> query,
      string sortBy,
      bool descending)
  {
    return sortBy.ToLowerInvariant() switch
    {
      "appointmentreference" => descending
          ? query.OrderByDescending(a => a.AppointmentReference).ThenByDescending(a => a.Id)
          : query.OrderBy(a => a.AppointmentReference).ThenBy(a => a.Id),
      "status" => descending
          ? query.OrderByDescending(a => a.Status).ThenByDescending(a => a.Id)
          : query.OrderBy(a => a.Status).ThenBy(a => a.Id),
      _ => descending
          ? query.OrderByDescending(a => a.ScheduledStart).ThenByDescending(a => a.Id)
          : query.OrderBy(a => a.ScheduledStart).ThenBy(a => a.Id)
    };
  }

  private static async Task<PagedResult<AppointmentSearchItem>> ProjectAsync(
      IQueryable<AppointmentOperationalListReadModel> query,
      AppointmentSearchCommand command,
      int totalCount,
      CancellationToken cancellationToken)
  {
    var items = await query.Select(a => new AppointmentSearchItem(
        a.Id, a.AppointmentReference, a.PatientId, a.PatientReference,
        a.PatientDisplayName, a.ReferralId, a.ReferralReference,
        a.AppointmentType, a.ScheduledStart, a.ScheduledEnd,
        a.Location, a.Status, a.CreatedAt)).ToListAsync(cancellationToken);
    return new PagedResult<AppointmentSearchItem>(
        items, command.Page, command.PageSize, totalCount,
        (int)Math.Ceiling(totalCount / (double)command.PageSize));
  }
}
