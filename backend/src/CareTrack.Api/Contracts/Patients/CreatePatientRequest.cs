using System.ComponentModel.DataAnnotations;

namespace CareTrack.Api.Contracts.Patients;

public sealed record CreatePatientRequest(
[Required]
[StringLength(20)]
string PatientReference,
[Required]
[StringLength(100)]
string FirstName,
[Required]
[StringLength(100)]
string LastName,
DateOnly DateOfBirth
);