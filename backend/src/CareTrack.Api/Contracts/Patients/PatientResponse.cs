namespace CareTrack.Api.Contracts.Patients;

public sealed record PatientResponse(
Guid Id,
string PatientReference,
string FirstName,
string LastName,
string FullName,
DateOnly DateOfBirth,
DateTime CreatedAt
);