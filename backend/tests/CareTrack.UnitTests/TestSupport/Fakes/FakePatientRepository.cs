using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.TestSupport.Fakes;

public class FakePatientRepository : IPatientRepository
{
  private readonly List<Patient> _patients = [];
  public byte[]? LastOriginalRowVersion { get; private set; }
  public Task<Patient?> GetByReferenceAsync(string patientReference, CancellationToken cancellationToken = default)
  {
    var patient = _patients.FirstOrDefault(patient => patient.PatientReference == patientReference);
    return Task.FromResult(patient);
  }
  public Task<Patient?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default)
  {
    var patient = _patients.FirstOrDefault(
        patient => patient.Id == id);

    return Task.FromResult(patient);
  }
  public Task<PagedResult<Patient>> SearchAsync(
    string? search,
    int page,
    int pageSize,
    string? sortBy,
    string? sortDirection,
    CancellationToken cancellationToken = default)
  {
    // Start with all patients.
    IEnumerable<Patient> query = _patients;

    // Apply search before counting and pagination.
    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(patient =>
          patient.PatientReference.Contains(
              search,
              StringComparison.OrdinalIgnoreCase) ||

          patient.FirstName.Contains(
              search,
              StringComparison.OrdinalIgnoreCase) ||

          patient.LastName.Contains(
              search,
              StringComparison.OrdinalIgnoreCase));
    }

    // Count all matching patients before pagination.
    var totalCount = query.Count();


    IOrderedEnumerable<Patient> orderedQuery = sortBy switch
    {
      "firstname" => sortDirection == "desc"
          ? query.OrderByDescending(p => p.FirstName)
          : query.OrderBy(p => p.FirstName),

      "patientreference" => sortDirection == "desc"
          ? query.OrderByDescending(p => p.PatientReference)
          : query.OrderBy(p => p.PatientReference),

      "createdat" => sortDirection == "desc"
          ? query.OrderByDescending(p => p.CreatedAt)
          : query.OrderBy(p => p.CreatedAt),

      _ => sortDirection == "desc"
          ? query.OrderByDescending(p => p.LastName)
          : query.OrderBy(p => p.LastName)
    };

    // Add a deterministic tie-breaker, then paginate.
    var items = orderedQuery
        .ThenBy(patient => patient.PatientReference)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    var totalPages = (totalCount + pageSize - 1) / pageSize;

    return Task.FromResult(new PagedResult<Patient>(items, page, pageSize, totalCount, totalPages));
  }

  public Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
  {

    _patients.Add(patient);
    return Task.CompletedTask;
  }

  public Task SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    return Task.CompletedTask;
  }
  public void SetOriginalRowVersion(
    Patient patient,
    byte[] rowVersion)
  {
    LastOriginalRowVersion = rowVersion;
  }
}
