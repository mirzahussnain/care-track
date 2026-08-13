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
   PatientSearchQuery query,
    CancellationToken cancellationToken = default)
  {
    if (query.Page < 1)
    {
      throw new ArgumentException("Page must be greater than or equal to 1.", nameof(query.Page));
    }

    if (query.PageSize < 1 || query.PageSize > MaximumPageSize)
    {
      throw new ArgumentException($"Page size must be between 1 and {MaximumPageSize}.", nameof(query.PageSize));
    }

    var trimmedSearch = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();

    var sortBy = query.SortBy.Trim().ToLowerInvariant();
    var sortDirection = query.SortDirection.Trim().ToLowerInvariant();

    var allowedSortFields = new[]{
      "lastname",
      "firstname",
      "patientreference",
      "createdat"
    };

    if (!allowedSortFields.Contains(sortBy))
    {
      throw new ArgumentException("Sort field must be one of: lastName, firstName, patientReference, createdAt.", nameof(query.SortBy));
    }

    if (sortDirection is not "asc" and not "desc")
    {
      throw new ArgumentException(
          "Sort direction must be 'asc' or 'desc'.",
          nameof(query.SortDirection));
    }
    return _patientRepository.SearchAsync(
     trimmedSearch,
     query.Page,
     query.PageSize,
     sortBy,
     sortDirection,
     cancellationToken);
  }
}