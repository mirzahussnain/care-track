using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;


public class FakeAppointmentRepository
    : IAppointmentRepository
{
  public List<Appointment> Appointments { get; }
      = [];

  public bool SchedulingConflictExists
  {
    get;
    set;
  }

  public Task<bool> HasSchedulingConflictAsync(
    Guid patientId,
    DateTime scheduledStart,
    DateTime scheduledEnd,
    Guid? excludeAppointmentId = null,
    CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
        SchedulingConflictExists);
  }


  public Task<Appointment?> GetByIdAsync(
      Guid id,
      CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
        Appointments.SingleOrDefault(
            appointment =>
                appointment.Id == id));
  }

  public Task<Appointment?> GetByReferenceAsync(
      string appointmentReference,
      CancellationToken cancellationToken = default)
  {
    return Task.FromResult(
        Appointments.SingleOrDefault(
            appointment =>
                appointment.AppointmentReference ==
                appointmentReference));
  }

  public Task<IReadOnlyList<Appointment>> GetByReferralIdAsync(
    Guid referralId,
    CancellationToken cancellationToken = default)
  {
    IReadOnlyList<Appointment> appointments =
        Appointments
            .Where(
                appointment =>
                    appointment.ReferralId == referralId)
            .OrderBy(
                appointment =>
                    appointment.ScheduledStart)
            .ThenBy(
                appointment =>
                    appointment.Id)
            .ToList();

    return Task.FromResult(
        appointments);
  }

  public Task<PagedResult<Appointment>> SearchAsync(
    AppointmentSearchCommand command,
    CancellationToken cancellationToken = default)
  {
    var items =
        Appointments.AsEnumerable();

    if (command.PatientId.HasValue)
    {
      items =
          items.Where(
              a => a.PatientId == command.PatientId.Value);
    }

    if (command.ReferralId.HasValue)
    {
      items =
          items.Where(
              a => a.ReferralId == command.ReferralId.Value);
    }

    if (command.Status.HasValue)
    {
      items =
          items.Where(
              a => a.Status == command.Status.Value);
    }

    if (command.AppointmentType.HasValue)
    {
      items =
          items.Where(
              a => a.AppointmentType ==
                   command.AppointmentType.Value);
    }

    if (!string.IsNullOrWhiteSpace(
            command.Location))
    {
      var location =
          command.Location.Trim();

      items =
          items.Where(
              a => a.Location.Contains(
                  location,
                  StringComparison.OrdinalIgnoreCase));
    }

    if (command.ScheduledFrom.HasValue
        && command.ScheduledTo.HasValue)
    {
      items =
          items.Where(
              a =>
                  a.ScheduledStart <
                      command.ScheduledTo.Value
                  &&
                  a.ScheduledEnd >
                      command.ScheduledFrom.Value);
    }
    else if (command.ScheduledFrom.HasValue)
    {
      items =
          items.Where(
              a =>
                  a.ScheduledEnd >
                  command.ScheduledFrom.Value);
    }
    else if (command.ScheduledTo.HasValue)
    {
      items =
          items.Where(
              a =>
                  a.ScheduledStart <
                  command.ScheduledTo.Value);
    }

    var totalCount =
        items.Count();

    var descending =
        command.SortDirection.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase);

    items =
        command.SortBy.ToLowerInvariant() switch
        {
          "scheduledend" =>
              descending
                  ? items
                      .OrderByDescending(
                          a => a.ScheduledEnd)
                      .ThenByDescending(
                          a => a.Id)
                  : items
                      .OrderBy(
                          a => a.ScheduledEnd)
                      .ThenBy(
                          a => a.Id),

          "createdat" =>
              descending
                  ? items
                      .OrderByDescending(
                          a => a.CreatedAt)
                      .ThenByDescending(
                          a => a.Id)
                  : items
                      .OrderBy(
                          a => a.CreatedAt)
                      .ThenBy(
                          a => a.Id),

          "appointmentreference" =>
              descending
                  ? items
                      .OrderByDescending(
                          a => a.AppointmentReference)
                      .ThenByDescending(
                          a => a.Id)
                  : items
                      .OrderBy(
                          a => a.AppointmentReference)
                      .ThenBy(
                          a => a.Id),

          "status" =>
              descending
                  ? items
                      .OrderByDescending(
                          a => a.Status)
                      .ThenByDescending(
                          a => a.Id)
                  : items
                      .OrderBy(
                          a => a.Status)
                      .ThenBy(
                          a => a.Id),

          _ =>
              descending
                  ? items
                      .OrderByDescending(
                          a => a.ScheduledStart)
                      .ThenByDescending(
                          a => a.Id)
                  : items
                      .OrderBy(
                          a => a.ScheduledStart)
                      .ThenBy(
                          a => a.Id)
        };

    var pagedItems =
        items
            .Skip(
                (command.Page - 1)
                * command.PageSize)
            .Take(
                command.PageSize)
            .ToList();

    var totalPages =
        (int)Math.Ceiling(
            totalCount /
            (double)command.PageSize);

    return Task.FromResult(
        new PagedResult<Appointment>(
            pagedItems,
            command.Page,
            command.PageSize,
            totalCount,
            totalPages));
  }

  public Task AddAsync(
      Appointment appointment,
      CancellationToken cancellationToken = default)
  {
    Appointments.Add(
        appointment);

    return Task.CompletedTask;
  }

  public Task SaveChangesAsync(
      CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }
}