using CareTrack.Application.Common.Models;

namespace CareTrack.Application.Appointments.SearchAppointments;

public interface IAppointmentSearchQuery
{
  Task<PagedResult<AppointmentSearchItem>> SearchAsync(
      AppointmentSearchCommand command,
      CancellationToken cancellationToken = default);
}
