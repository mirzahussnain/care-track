using CareTrack.Domain.Entities;
namespace CareTrack.Application.Common.Interfaces;

public interface IPatientRepository
{
  Task<Patient?> GetByReferenceAsync(
  string patientReference,
  CancellationToken cancellationToken = default
  );

  Task AddAsync(Patient patient, CancellationToken cancellationToken = default);

}