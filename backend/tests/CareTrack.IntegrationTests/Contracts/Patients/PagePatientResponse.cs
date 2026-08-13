namespace CareTrack.IntegrationTests.Contracts.Patients;

public sealed record PagedPatientResponse(
  IReadOnlyList<PatientResponse> Items,
  int Page,
  int PageSize,
  int TotalCount,
  int TotalPages);