using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace CareTrack.Application.Patients.UpdatePatient;

public class UpdatePatientService
{
  private readonly ILogger<UpdatePatientService> _logger;
  private readonly IPatientRepository _patientRepository;
  public UpdatePatientService(IPatientRepository patientRepository, ILogger<UpdatePatientService> logger)
  {
    _patientRepository = patientRepository;
    _logger = logger;
  }
  public async Task<Patient> ExecuteAsync(UpdatePatientCommand command, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(command.FirstName))
    {
      throw new ArgumentException("First name cannot be empty.", nameof(command.FirstName));
    }
    if (string.IsNullOrWhiteSpace(command.LastName))
    {
      throw new ArgumentException("Last name cannot be empty.", nameof(command.LastName));
    }

    if (command.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
    {
      throw new ArgumentException("Date of birth cannot be in the future.", nameof(command.DateOfBirth));
    }

    // Only query persistence once basic validation succeeds.
    _logger.LogInformation("Fetching patient with id '{id}'", command.Id);
    var patient = await _patientRepository.GetByIdAsync(command.Id, cancellationToken);
    if (patient is null)
    {
      _logger.LogWarning("Patient with id '{id}' not found", command.Id);
      throw new NotFoundException($"Patient with id '{command.Id}' was not found.");

    }
    _logger.LogInformation("Patient with id '{id}' found. Updating patient...", command.Id);
    // Tell EF which version the client originally read.
    _patientRepository.SetOriginalRowVersion(
    patient,
    command.RowVersion);

    // Domain mutation
    patient.UpdateName(command.FirstName, command.LastName);
    patient.UpdateDateOfBirth(command.DateOfBirth);

    // Persistence.
    await _patientRepository.SaveChangesAsync(cancellationToken);

    _logger.LogInformation("Patient with id '{id}' updated successfully", command.Id);
    return patient;
  }
}