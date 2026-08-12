using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance.Repositories;

public class PatientRepository : IPatientRepository
{
  private readonly CareTrackDbContext _dbContext;
  public PatientRepository(CareTrackDbContext dbContext)
  {
    _dbContext = dbContext;
  }
  public async Task<Patient?> GetByReferenceAsync(
  string patientReference, CancellationToken cancellationToken = default
  )
  {
    return await _dbContext.Patients.FirstOrDefaultAsync(
    patient => patient.PatientReference == patientReference, cancellationToken
    );
  }
  public async Task<Patient?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    return await _dbContext.Patients
        .FirstOrDefaultAsync(
            patient => patient.Id == id,
            cancellationToken);
  }

  public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
  {
    await _dbContext.Patients.AddAsync(patient, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}