namespace CareTrack.Application.Patients.SearchPatients;

public sealed record PatientSearchQuery(
string? Search = null,
int Page = 1,
int PageSize = 20,
string SortBy = "lastName",
string SortDirection = "asc"
);