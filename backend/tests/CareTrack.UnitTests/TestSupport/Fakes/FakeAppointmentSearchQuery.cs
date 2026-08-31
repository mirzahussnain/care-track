using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Models;

namespace CareTrack.UnitTests.TestSupport.Fakes;

public sealed class FakeAppointmentSearchQuery : IAppointmentSearchQuery
{
  public Task<PagedResult<AppointmentSearchItem>> SearchAsync(
      AppointmentSearchCommand command,
      CancellationToken cancellationToken = default)
  {
    return Task.FromResult(new PagedResult<AppointmentSearchItem>(
        [], command.Page, command.PageSize, 0, 0));
  }
}
