using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
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