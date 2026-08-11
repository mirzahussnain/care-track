namespace CareTrack.Api.Contracts.Patients;

public sealed record CreatePatientRequest(
string PatientReference,
string FirstName,
string LastName,
DateOnly DateOfBirth
);