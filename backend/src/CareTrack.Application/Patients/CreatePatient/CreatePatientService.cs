using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Patients.CreatePatient;

public class CreatePatientService
{
  private readonly IPatientRepository _patientRepository;
  private readonly ILogger<CreatePatientService> _logger;

  public CreatePatientService(
      IPatientRepository patientRepository,
      ILogger<CreatePatientService> logger)
  {
    _patientRepository = patientRepository;
    _logger = logger;
  }

  public async Task<Patient> ExecuteAsync(
      CreatePatientCommand command,
      CancellationToken cancellationToken = default)
  {
    var patientReference = command.PatientReference.Trim();

    var existingPatient =
        await _patientRepository.GetByReferenceAsync(
            patientReference,
            cancellationToken);

    if (existingPatient is not null)
    {
      _logger.LogWarning(
          "Patient creation rejected because reference {PatientReference} already exists",
          patientReference);

      throw new ConflictException(
          $"A patient with reference '{patientReference}' already exists.");
    }

    _logger.LogInformation(
        "Creating patient with reference {PatientReference}",
        patientReference);

    var patient = new Patient(
        patientReference,
        command.FirstName,
        command.LastName,
        command.DateOfBirth);

    await _patientRepository.AddAsync(
        patient,
        cancellationToken);

    _logger.LogInformation(
        "Patient {PatientId} created successfully",
        patient.Id);

    return patient;
  }
}