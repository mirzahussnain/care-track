using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;
namespace CareTrack.Application.Patients.CreatePatient;

public class CreatePatientService
{
  private readonly ILogger<CreatePatientService> _logger;
  private readonly IPatientRepository _patientRepository;
  public CreatePatientService(IPatientRepository patientRepository, ILogger<CreatePatientService> logger)
  {
    _logger = logger;
    _patientRepository = patientRepository;
  }

  public async Task<Patient> ExecuteAsync(
  CreatePatientCommand command,
  CancellationToken cancellationToken = default

  )
  {
    _logger.LogInformation("Finding patient with reference {PatientReference}", command.PatientReference);
    var existingPatient = await _patientRepository.GetByReferenceAsync(command.PatientReference, cancellationToken);
    if (existingPatient is not null)
    {
      throw new ConflictException($"A patient with reference '{command.PatientReference}' already exists.");
    }
    _logger.LogInformation("No Patient reference id '{id}' is found.", command.PatientReference);
    _logger.LogInformation("Creating new patient with reference {PatientReference}", command.PatientReference);
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