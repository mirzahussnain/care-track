using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
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

  public async Task<PagedResult<Patient>> SearchAsync(
    string? search,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default)
  {
    IQueryable<Patient> query = _dbContext.Patients.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(patient =>
        patient.PatientReference.Contains(search) ||
        patient.FirstName.Contains(search) ||
        patient.LastName.Contains(search));
    }

    var totalCount = await query.CountAsync(cancellationToken);
    var items = await query
      .OrderBy(patient => patient.LastName)
      .ThenBy(patient => patient.FirstName)
      .ThenBy(patient => patient.PatientReference)
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    var totalPages = (totalCount + pageSize - 1) / pageSize;

    return new PagedResult<Patient>(items, page, pageSize, totalCount, totalPages);
  }

  public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
  {
    await _dbContext.Patients.AddAsync(patient, cancellationToken);
    await _dbContext.SaveChangesAsync(cancellationToken);
  }
}