using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Patients.CreatePatient;

public class CreatePatientService
{
  private readonly IPatientRepository _patientRepository;
  public CreatePatientService(IPatientRepository patientRepository)
  {
    _patientRepository = patientRepository;
  }

  public async Task<Patient> ExecuteAsync(
  CreatePatientCommand command,
  CancellationToken cancellationToken = default

  )
  {
    var existingPatient = await _patientRepository.GetByReferenceAsync(command.PatientReference, cancellationToken);
    if (existingPatient is not null)
    {
      throw new ConflictException($"A patient with reference '{command.PatientReference}' already exists.");
    }
    var patient = new Patient(
    command.PatientReference,
    command.FirstName,
    command.LastName,
    command.DateOfBirth

    );
    await _patientRepository.AddAsync(patient, cancellationToken);
    return patient;
  }
}