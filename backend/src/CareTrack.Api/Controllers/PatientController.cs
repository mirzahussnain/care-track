using CareTrack.Api.Contracts.Patients;
using CareTrack.Application.Patients.CreatePatient;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
  private readonly CreatePatientService _createPatientService;
  public PatientsController(CreatePatientService createPatientService)
  {
    _createPatientService = createPatientService;
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

    return Created($"/api/patients/{patient.Id}", response);
  }
}