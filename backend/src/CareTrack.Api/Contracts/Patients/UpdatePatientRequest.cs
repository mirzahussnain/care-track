using System.ComponentModel.DataAnnotations;
namespace CareTrack.Api.Contracts.Patients;

public sealed record UpdatePatientRequest(
    [Required]
    [StringLength(100)]
    string FirstName,

    [Required]
    [StringLength(100)]
    string LastName,

    DateOnly DateOfBirth,
    [Required]
    string RowVersion
);

