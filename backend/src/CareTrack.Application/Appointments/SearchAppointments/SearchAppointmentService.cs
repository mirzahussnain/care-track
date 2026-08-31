using CareTrack.Application.Common.Models;

namespace CareTrack.Application.Appointments.SearchAppointments;


public sealed class SearchAppointmentsService
{
  private readonly IAppointmentSearchQuery _appointmentSearchQuery;

  public SearchAppointmentsService(
      IAppointmentSearchQuery appointmentSearchQuery)
  {
    _appointmentSearchQuery = appointmentSearchQuery;
  }

  public async Task<PagedResult<AppointmentSearchItem>>
      ExecuteAsync(
          AppointmentSearchCommand command,
          CancellationToken cancellationToken = default)
  {
    Validate(command);

    return await _appointmentSearchQuery.SearchAsync(
        command,
        cancellationToken);
  }

  private static void Validate(
      AppointmentSearchCommand command)
  {
    if (command.Page < 1)
    {
      throw new ArgumentException(
          "Page must be at least 1.");
    }

    if (command.PageSize < 1
        || command.PageSize > 100)
    {
      throw new ArgumentException(
          "Page size must be between 1 and 100.");
    }

    if (command.ScheduledFrom.HasValue
        && command.ScheduledTo.HasValue
        && command.ScheduledTo <= command.ScheduledFrom)
    {
      throw new ArgumentException(
          "ScheduledTo must be after ScheduledFrom.");
    }

    var allowedSortFields =
        new[]
        {
                "scheduledstart",
                "scheduledend",
                "createdat",
                "appointmentreference",
                "status"
        };

    if (!allowedSortFields.Contains(
            command.SortBy.ToLowerInvariant()))
    {
      throw new ArgumentException(
          "Invalid appointment sort field.");
    }

    if (!command.SortDirection.Equals(
            "asc",
            StringComparison.OrdinalIgnoreCase)
        &&
        !command.SortDirection.Equals(
            "desc",
            StringComparison.OrdinalIgnoreCase))
    {
      throw new ArgumentException(
          "Sort direction must be 'asc' or 'desc'.");
    }
  }
}

