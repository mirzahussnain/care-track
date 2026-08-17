using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance.Repositories;

public class AppointmentRepository
    : IAppointmentRepository
{
  private readonly CareTrackDbContext _dbContext;

  public AppointmentRepository(
      CareTrackDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<Appointment?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    return _dbContext.Appointments
        .SingleOrDefaultAsync(
            appointment =>
                appointment.Id == id,
            cancellationToken);
  }

  public Task<Appointment?> GetByReferenceAsync(
      string appointmentReference,
      CancellationToken cancellationToken = default)
  {
    return _dbContext.Appointments
        .AsNoTracking()
        .SingleOrDefaultAsync(
            appointment =>
                appointment.AppointmentReference ==
                appointmentReference,
            cancellationToken);
  }

  public async Task<PagedResult<Appointment>> SearchAsync(
    AppointmentSearchCommand command,
    CancellationToken cancellationToken = default)
  {
    IQueryable<Appointment> appointments =
        _dbContext.Appointments
            .AsNoTracking();

    if (command.PatientId.HasValue)
    {
      appointments =
          appointments.Where(
              a => a.PatientId == command.PatientId.Value);
    }

    if (command.ReferralId.HasValue)
    {
      appointments =
          appointments.Where(
              a => a.ReferralId == command.ReferralId.Value);
    }

    if (command.Status.HasValue)
    {
      appointments =
          appointments.Where(
              a => a.Status == command.Status.Value);
    }

    if (command.AppointmentType.HasValue)
    {
      appointments =
          appointments.Where(
              a => a.AppointmentType ==
                   command.AppointmentType.Value);
    }

    if (!string.IsNullOrWhiteSpace(command.Location))
    {
      var location =
          command.Location.Trim();

      appointments =
          appointments.Where(
              a => a.Location.Contains(location));
    }

    if (command.ScheduledFrom.HasValue
    && command.ScheduledTo.HasValue)
    {
      var from =
          command.ScheduledFrom.Value;

      var to =
          command.ScheduledTo.Value;

      appointments =
          appointments.Where(
              a =>
                  a.ScheduledStart < to
                  && a.ScheduledEnd > from);
    }
    else if (command.ScheduledFrom.HasValue)
    {
      var from =
          command.ScheduledFrom.Value;

      appointments =
          appointments.Where(
              a => a.ScheduledEnd > from);
    }
    else if (command.ScheduledTo.HasValue)
    {
      var to =
          command.ScheduledTo.Value;

      appointments =
          appointments.Where(
              a => a.ScheduledStart < to);
    }

    var totalCount = await appointments.CountAsync(
        cancellationToken);

    var descending =
    command.SortDirection.Equals(
        "desc",
        StringComparison.OrdinalIgnoreCase);

    appointments =
        command.SortBy.ToLowerInvariant() switch
        {
          "scheduledend" =>
          descending
              ? appointments
                  .OrderByDescending(a => a.ScheduledEnd)
                  .ThenByDescending(a => a.Id)
              : appointments
                  .OrderBy(a => a.ScheduledEnd)
                  .ThenBy(a => a.Id),

          "createdat" =>
          descending
              ? appointments
                  .OrderByDescending(a => a.CreatedAt)
                  .ThenByDescending(a => a.Id)
              : appointments
                  .OrderBy(a => a.CreatedAt)
                  .ThenBy(a => a.Id),

          "appointmentreference" =>
          descending
              ? appointments
                  .OrderByDescending(a => a.AppointmentReference)
                  .ThenByDescending(a => a.Id)
              : appointments
                  .OrderBy(a => a.AppointmentReference)
                  .ThenBy(a => a.Id),

          "status" =>
          descending
              ? appointments
                  .OrderByDescending(a => a.Status)
                  .ThenByDescending(a => a.Id)
              : appointments
                  .OrderBy(a => a.Status)
                  .ThenBy(a => a.Id),

          _ =>
          descending
              ? appointments
                  .OrderByDescending(a => a.ScheduledStart)
                  .ThenByDescending(a => a.Id)
              : appointments
                  .OrderBy(a => a.ScheduledStart)
                  .ThenBy(a => a.Id)
        };

    var page =
command.Page;

    var pageSize =
        command.PageSize;

    var items =
        await appointments
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    var totalPages =
        (int)Math.Ceiling(
            totalCount / (double)pageSize);

    return new PagedResult<Appointment>(
        items,
        page,
        pageSize,
        totalCount,
        totalPages);
  }

  public Task<bool> HasSchedulingConflictAsync(
    Guid patientId,
    DateTime scheduledStart,
    DateTime scheduledEnd,
    Guid? excludeAppointmentId = null,
    CancellationToken cancellationToken = default)
  {
    var query =
        _dbContext.Appointments
            .AsNoTracking()
            .Where(a =>
                a.PatientId == patientId
                &&
                a.Status != AppointmentStatus.Cancelled
                &&
                a.Status != AppointmentStatus.DidNotAttend
                &&
                a.ScheduledStart < scheduledEnd
                &&
                a.ScheduledEnd > scheduledStart);

    if (excludeAppointmentId.HasValue)
    {
      query =
          query.Where(
              a => a.Id != excludeAppointmentId.Value);
    }

    return query.AnyAsync(
        cancellationToken);
  }


  public async Task AddAsync(
      Appointment appointment,
      CancellationToken cancellationToken = default)
  {
    await _dbContext.Appointments.AddAsync(
        appointment,
        cancellationToken);

    await _dbContext.SaveChangesAsync(
        cancellationToken);
  }

  public Task SaveChangesAsync(
      CancellationToken cancellationToken = default)
  {
    return _dbContext.SaveChangesAsync(
        cancellationToken);
  }
}