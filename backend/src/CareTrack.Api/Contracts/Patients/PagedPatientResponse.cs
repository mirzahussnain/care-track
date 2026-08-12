namespace CareTrack.Api.Contracts.Patients;

public sealed record PagedPatientResponse(
  IReadOnlyList<PatientResponse> Items,
  int Page,
  int PageSize,
  int TotalCount,
  int TotalPages);