using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.Fakes;

public class FakePatientRepository : IPatientRepository
{
  private readonly List<Patient> _patients = [];
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
    CancellationToken cancellationToken = default)
  {
    IEnumerable<Patient> query = _patients;

    if (!string.IsNullOrWhiteSpace(search))
    {
      query = query.Where(patient =>
        patient.PatientReference.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        patient.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
        patient.LastName.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    var totalCount = query.Count();
    var items = query
      .OrderBy(patient => patient.LastName)
      .ThenBy(patient => patient.FirstName)
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
}
