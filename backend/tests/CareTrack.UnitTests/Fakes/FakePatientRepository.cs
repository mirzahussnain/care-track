using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.UnitTests.Fakes;

public class FakePatientRepository : IPatientRepository
{
  private readonly List<Patient> _patients = [];
  public Task<Patient?> GetByReferenceAsync(string patientReference, CancellationToken cancellationToken = default)
  {
    var patient = _patients.FirstOrDefault(patient => patient.PatientReference == patientReference);
    return Task.FromResult(patient);
  }
  public Task AddAsync(Patient patient, CancellationToken cancellationToken = default)
  {

    _patients.Add(patient);
    return Task.CompletedTask;
  }
}
