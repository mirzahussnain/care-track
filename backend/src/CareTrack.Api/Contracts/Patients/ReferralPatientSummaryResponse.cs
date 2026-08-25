namespace CareTrack.Api.Contracts.Patients;

public sealed record ReferralPatientSummaryResponse(
    Guid Id,
    string PatientReference,
    string FullName,
    DateOnly DateOfBirth);

public sealed record PagedReferralPatientSummaryResponse(
    IReadOnlyList<ReferralPatientSummaryResponse> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages);
