using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Patients.SearchPatients;

public class SearchPatientsService
{
  private const int MaximumPageSize = 100;
  private readonly IPatientRepository _patientRepository;

  public SearchPatientsService(IPatientRepository patientRepository)
  {
    _patientRepository = patientRepository;
  }

  public Task<PagedResult<Patient>> ExecuteAsync(
    string? search,
    int page = 1,
    int pageSize = 20,
    CancellationToken cancellationToken = default)
  {
    if (page < 1)
    {
      throw new ArgumentException("Page must be greater than or equal to 1.", nameof(page));
    }

    if (pageSize < 1 || pageSize > MaximumPageSize)
    {
      throw new ArgumentException($"Page size must be between 1 and {MaximumPageSize}.", nameof(pageSize));
    }

    var trimmedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

    return _patientRepository.SearchAsync(trimmedSearch, page, pageSize, cancellationToken);
  }
}