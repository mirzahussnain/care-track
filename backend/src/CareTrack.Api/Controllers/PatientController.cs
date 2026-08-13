using CareTrack.Api.Contracts.Patients;
using CareTrack.Api.Mappings;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.Application.Patients.GetPatient;
using CareTrack.Application.Patients.SearchPatients;
using CareTrack.Application.Patients.UpdatePatient;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/patients")]
public class PatientsController : ControllerBase
{
  private readonly CreatePatientService _createPatientService;
  private readonly GetPatientService _getPatientService;
  private readonly SearchPatientsService _searchPatientsService;

  private readonly UpdatePatientService _updatePatiientService;
  public PatientsController(
    CreatePatientService createPatientService,
    GetPatientService getPatientService,
    SearchPatientsService searchPatientsService,
    UpdatePatientService updatePatiientService)
  {
    _createPatientService = createPatientService;
    _getPatientService = getPatientService;
    _searchPatientsService = searchPatientsService;
    _updatePatiientService = updatePatiientService;
  }

  [HttpGet]
  public async Task<ActionResult<PagedPatientResponse>> GetPatients(
    [FromQuery] string? search,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string sortBy = "lastName",
    [FromQuery] string sortDirection = "asc",
    CancellationToken cancellationToken = default)
  {
    var query = new PatientSearchQuery(
        search,
        page,
        pageSize,
        sortBy,
        sortDirection);

    var result = await _searchPatientsService.ExecuteAsync(
        query,
        cancellationToken);
    var response = new PagedPatientResponse(
      result.Items.Select(patient => patient.ToResponse()).ToList(),
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

    var response = patient.ToResponse();

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
    var response = patient.ToResponse();

    return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, response);
  }

  [HttpPut("{id:guid}")]
  public async Task<ActionResult<PatientResponse>> UpdatePatient(Guid id, UpdatePatientRequest request, CancellationToken cancellationToken)
  {
    byte[] rowVersion;
    try
    {
      rowVersion = Convert.FromBase64String(request.RowVersion);
    }
    catch (FormatException)
    {
      throw new ArgumentException("RowVersion must be a valid Base64 value.", nameof(request.RowVersion));
    }

    var command = new UpdatePatientCommand(id, request.FirstName, request.LastName, request.DateOfBirth, rowVersion);
    var patient = await _updatePatiientService.ExecuteAsync(command, cancellationToken);
    var response = patient.ToResponse();
    return Ok(response);
  }
}