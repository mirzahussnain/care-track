using CareTrack.Api.Contracts.Patients;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.Application.Patients.GetPatient;
using CareTrack.Application.Patients.SearchPatients;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
  private readonly CreatePatientService _createPatientService;
  private readonly GetPatientService _getPatientService;
  private readonly SearchPatientsService _searchPatientsService;
  public PatientsController(
    CreatePatientService createPatientService,
    GetPatientService getPatientService,
    SearchPatientsService searchPatientsService)
  {
    _createPatientService = createPatientService;
    _getPatientService = getPatientService;
    _searchPatientsService = searchPatientsService;
  }

  [HttpGet]
  public async Task<ActionResult<PagedPatientResponse>> GetPatients(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    CancellationToken cancellationToken = default)
  {
    var result = await _searchPatientsService.ExecuteAsync(search, page, pageSize, cancellationToken);
    var response = new PagedPatientResponse(
      result.Items.Select(patient => new PatientResponse(
        patient.Id,
        patient.PatientReference,
        patient.FirstName,
        patient.LastName,
        patient.FullName,
        patient.DateOfBirth,
        patient.CreatedAt)).ToList(),
      result.Page,
      result.PageSize,
      result.TotalCount,
      result.TotalPages);

    return Ok(response);
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