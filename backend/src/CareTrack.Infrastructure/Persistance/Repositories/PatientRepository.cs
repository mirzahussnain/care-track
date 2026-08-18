using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace CareTrack.Infrastructure.Persistance.Repositories;

public class PatientRepository : IPatientRepository
{
  private readonly CareTrackDbContext _dbContext;
  public PatientRepository(CareTrackDbContext dbContext)
  {
    _dbContext = dbContext;
  }
  public void SetOriginalRowVersion(
    Patient patient,
    byte[] rowVersion)
  {
    _dbContext
        .Entry(patient)
        .Property(p => p.RowVersion)
        .OriginalValue = rowVersion;
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
    string sortBy,
    string sortDirection,
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

    var descending = sortDirection == "desc";
    query = sortBy switch
    {
      "firstname" => descending ? query.OrderByDescending(patient => patient.FirstName) : query.OrderBy(patient => patient.FirstName),
      "patientreference" => descending ? query.OrderByDescending(patient => patient.PatientReference) : query.OrderBy(patient => patient.PatientReference),
      "createdat" => descending ? query.OrderByDescending(patient => patient.CreatedAt) : query.OrderBy(patient => patient.CreatedAt),
      _ => descending ? query.OrderByDescending(patient => patient.LastName) : query.OrderBy(patient => patient.LastName)
    };

    query = ((IOrderedQueryable<Patient>)query).ThenBy(patient => patient.PatientReference);
    var items = await query
      .Skip((page - 1) * pageSize)
      .Take(pageSize)
      .ToListAsync(cancellationToken);

    var totalPages = (totalCount + pageSize - 1) / pageSize;

    return new PagedResult<Patient>(items, page, pageSize, totalCount, totalPages);
  }

  public async Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
  {
    await _dbContext.Patients.AddAsync(patient, cancellationToken);

    try
    {
      await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException exception)
        when (IsDuplicatePatientReference(exception))
    {
      throw new ConflictException(
          $"A patient with reference '{patient.PatientReference}' already exists.");
    }
  }

  public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      await _dbContext.SaveChangesAsync(
          cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      throw new ConcurrencyException(
          "The patient was modified by another user. Reload the latest data and try again.",
          ex);
    }

  }
  private static bool IsDuplicatePatientReference(
      DbUpdateException exception)
  {
    return exception.InnerException is SqlException sqlException
        && (sqlException.Number == 2601 || sqlException.Number == 2627)
        && sqlException.Message.Contains(
            "IX_Patients_PatientReference",
            StringComparison.OrdinalIgnoreCase);
  }
}