using CareTrack.Api.Contracts.Patients;
using CareTrack.Domain.Entities;

namespace CareTrack.Api.Mappings;

public static class PatientMappings
{
  public static PatientResponse ToResponse(
      this Patient patient)
  {
    return new PatientResponse(
        patient.Id,
        patient.PatientReference,
        patient.FirstName,
        patient.LastName,
        patient.FullName,
        patient.DateOfBirth,
        patient.CreatedAt,
        Convert.ToBase64String(
            patient.RowVersion));
  }
}