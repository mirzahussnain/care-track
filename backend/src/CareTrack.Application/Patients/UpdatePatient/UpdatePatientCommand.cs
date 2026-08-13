namespace CareTrack.Application.Patients.UpdatePatient;

public sealed record UpdatePatientCommand(Guid Id, string FirstName, string LastName, DateOnly DateOfBirth, byte[] RowVersion);