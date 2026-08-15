using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

public class FakeAppointmentRepository
    : IAppointmentRepository
{
  public List<Appointment> Appointments { get; }
      = [];

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