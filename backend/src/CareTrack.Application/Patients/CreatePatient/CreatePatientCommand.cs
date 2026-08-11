namespace CareTrack.Application.Patients.CreatePatient;

public sealed record CreatePatientCommand(
string PatientReference, string FirstName, string LastName, DateOnly DateOfBirth
);