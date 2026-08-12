using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;


namespace CareTrack.Application.Patients.GetPatient;

public class GetPatientService
{
  private readonly IPatientRepository _patientRepository;
  public GetPatientService(IPatientRepository patientRepository)
  {
    _patientRepository = patientRepository;
  }
  public async Task<Patient> ExecuteAsync(Guid id, CancellationToken cancellationToken = default)
  {
    var patient = await _patientRepository.GetByIdAsync(id, cancellationToken);
    if (patient is null)
    {
      throw new NotFoundException($"Patient with id '{id}' was not found.");
    }
    return patient;
  }
}