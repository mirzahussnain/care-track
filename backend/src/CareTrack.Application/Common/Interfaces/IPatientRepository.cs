using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
namespace CareTrack.Application.Common.Interfaces;

public interface IPatientRepository
{
  Task<Patient?> GetByReferenceAsync(
  string patientReference,
  CancellationToken cancellationToken = default
  );

  Task<Patient?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken = default);
  Task<PagedResult<Patient>> SearchAsync(
    string? search,
    int page,
    int pageSize,
    CancellationToken cancellationToken = default);

  Task AddAsync(Patient patient, CancellationToken cancellationToken = default);
}