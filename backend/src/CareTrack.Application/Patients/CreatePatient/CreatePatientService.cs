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
    var existingPatient =
        await _patientRepository.GetByReferenceAsync(
            command.PatientReference,
            cancellationToken);

    if (existingPatient is not null)
    {
      _logger.LogWarning(
          "Patient creation rejected because reference {PatientReference} already exists",
          command.PatientReference);

      throw new ConflictException(
          $"A patient with reference '{command.PatientReference}' already exists.");
    }

    _logger.LogInformation(
        "Creating patient with reference {PatientReference}",
        command.PatientReference);

    var patient = new Patient(
        command.PatientReference,
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