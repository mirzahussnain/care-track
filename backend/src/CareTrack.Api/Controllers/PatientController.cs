using CareTrack.Api.Contracts.Patients;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.Application.Patients.GetPatient;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
  private readonly CreatePatientService _createPatientService;
  private readonly GetPatientService _getPatientService;
  public PatientsController(CreatePatientService createPatientService, GetPatientService getPatientService)
  {
    _createPatientService = createPatientService;
    _getPatientService = getPatientService;
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<PatientResponse>> GetPatient(
    Guid id,
    CancellationToken cancellationToken)
  {
    var patient = await _getPatientService.ExecuteAsync(
        id,
        cancellationToken);

    var response = new PatientResponse(
        patient.Id,
        patient.PatientReference,
        patient.FirstName,
        patient.LastName,
        patient.FullName,
        patient.DateOfBirth,
        patient.CreatedAt);

    return Ok(response);
  }


  [HttpPost]
  public async Task<ActionResult<PatientResponse>> CreatePatient(CreatePatientRequest request, CancellationToken cancellationToken)
  {
    var command = new CreatePatientCommand(
    request.PatientReference,
    request.FirstName,
    request.LastName,
    request.DateOfBirth
    );

    var patient = await _createPatientService.ExecuteAsync(command, cancellationToken);
    var response = new PatientResponse(
    patient.Id,
    patient.PatientReference,
    patient.FirstName,
    patient.LastName,
    patient.FullName,
    patient.DateOfBirth,
    patient.CreatedAt
    );

    return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, response);
  }
}