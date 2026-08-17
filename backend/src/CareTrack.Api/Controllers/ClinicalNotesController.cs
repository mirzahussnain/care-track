using CareTrack.Api.Contracts.ClinicalNotes;
using CareTrack.Application.ClinicalNotes.Common;
using CareTrack.Application.ClinicalNotes.CreateClinicalNote;
using CareTrack.Application.ClinicalNotes.GetClinicalNoteById;
using CareTrack.Application.ClinicalNotes.GetClinicalNotesByAppointment;
using CareTrack.Application.ClinicalNotes.UpdateClinicalNote;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
public sealed class ClinicalNotesController
    : ControllerBase
{
  private readonly CreateClinicalNoteService
      _createClinicalNoteService;

  private readonly GetClinicalNoteByIdService
      _getClinicalNoteByIdService;

  private readonly GetClinicalNotesByAppointmentService
      _getClinicalNotesByAppointmentService;

  private readonly UpdateClinicalNoteService
      _updateClinicalNoteService;

  public ClinicalNotesController(
      CreateClinicalNoteService createClinicalNoteService,
      GetClinicalNoteByIdService getClinicalNoteByIdService,
      GetClinicalNotesByAppointmentService getClinicalNotesByAppointmentService,
      UpdateClinicalNoteService updateClinicalNoteService)
  {
    _createClinicalNoteService =
        createClinicalNoteService;

    _getClinicalNoteByIdService =
        getClinicalNoteByIdService;

    _getClinicalNotesByAppointmentService =
        getClinicalNotesByAppointmentService;

    _updateClinicalNoteService =
        updateClinicalNoteService;
  }

  [HttpPost(
    "/api/appointments/{appointmentId:guid}/clinical-notes")]
  public async Task<ActionResult<ClinicalNoteResponse>>
    Create(
        Guid appointmentId,
        [FromBody] CreateClinicalNoteRequest request,
        CancellationToken cancellationToken)
  {
    var command =
        new CreateClinicalNoteCommand(
            appointmentId,
            request.Content,
            request.CreatedBy);

    var result =
        await _createClinicalNoteService.ExecuteAsync(
            command,
            cancellationToken);

    var response =
        ToResponse(result);

    return CreatedAtAction(
        nameof(GetById),
        new { id = response.Id },
        response);
  }

  [HttpGet(
    "/api/clinical-notes/{id:guid}")]
  public async Task<ActionResult<ClinicalNoteResponse>>
    GetById(
        Guid id,
        CancellationToken cancellationToken)
  {
    var result =
        await _getClinicalNoteByIdService.ExecuteAsync(
            id,
            cancellationToken);

    return Ok(
        ToResponse(result));
  }

  [HttpGet(
    "/api/appointments/{appointmentId:guid}/clinical-notes")]
  public async Task<
    ActionResult<IReadOnlyList<ClinicalNoteResponse>>>
    GetByAppointment(
        Guid appointmentId,
        CancellationToken cancellationToken)
  {
    var results =
        await _getClinicalNotesByAppointmentService
            .ExecuteAsync(
                appointmentId,
                cancellationToken);

    var response =
        results
            .Select(ToResponse)
            .ToList();

    return Ok(
        response);
  }

  [HttpPut(
    "/api/clinical-notes/{id:guid}")]
  public async Task<ActionResult<ClinicalNoteResponse>>
    Update(
        Guid id,
        [FromBody] UpdateClinicalNoteRequest request,
        CancellationToken cancellationToken)
  {
    var command =
        new UpdateClinicalNoteCommand(
            id,
            request.Content);

    var result =
        await _updateClinicalNoteService.ExecuteAsync(
            command,
            cancellationToken);

    return Ok(
        ToResponse(result));
  }

  private static ClinicalNoteResponse ToResponse(
    ClinicalNoteResult result)
  {
    return new ClinicalNoteResponse(
        result.Id,
        result.AppointmentId,
        result.Content,
        result.CreatedBy,
        result.CreatedAt,
        result.UpdatedAt);
  }

}