using CareTrack.Api.Contracts.Appointments;
using CareTrack.Application.Appointments.CancelAppointment;
using CareTrack.Application.Appointments.CheckInAppointment;
using CareTrack.Application.Appointments.CompleteAppointment;
using CareTrack.Application.Appointments.CreateAppointment;
using CareTrack.Application.Appointments.DidNotAttendAppointment;
using CareTrack.Application.Appointments.StartAppointment;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
  private readonly CreateAppointmentService _createAppointmentService;

  private readonly CheckInAppointmentService _checkInAppointmentService;
  private readonly StartAppointmentService _startAppointmentService;
  private readonly CompleteAppointmentService _completeAppointmentService;
  private readonly CancelAppointmentService _cancelAppointmentService;
  private readonly MarkAppointmentDidNotAttendService _didNotAttendService;
  public AppointmentsController(
      CreateAppointmentService createAppointmentService,
       CheckInAppointmentService checkInAppointmentService,
    StartAppointmentService startAppointmentService,
    CompleteAppointmentService completeAppointmentService,
    CancelAppointmentService cancelAppointmentService,
    MarkAppointmentDidNotAttendService didNotAttendService)
  {
    _createAppointmentService = createAppointmentService;
    _checkInAppointmentService = checkInAppointmentService;
    _startAppointmentService = startAppointmentService;
    _completeAppointmentService = completeAppointmentService;
    _cancelAppointmentService = cancelAppointmentService;
    _didNotAttendService = didNotAttendService;

  }

  private static AppointmentResponse ToResponse(
    Appointment appointment)
  {
    return new AppointmentResponse(
        appointment.Id,
        appointment.AppointmentReference,
        appointment.PatientId,
        appointment.ReferralId,
        appointment.AppointmentType,
        appointment.ScheduledStart,
        appointment.ScheduledEnd,
        appointment.Location,
        appointment.Status,
        appointment.CreatedAt,
        appointment.UpdatedAt,
        appointment.CheckedInAt,
        appointment.StartedAt,
        appointment.CompletedAt,
        appointment.CancelledAt,
        appointment.DidNotAttendAt);
  }

  private static AppointmentResponse ToResponse(
    CreateAppointmentResult result)
  {
    return new AppointmentResponse(
        result.Id,
        result.AppointmentReference,
        result.PatientId,
        result.ReferralId,
        result.AppointmentType,
        result.ScheduledStart,
        result.ScheduledEnd,
        result.Location,
        result.Status,
        result.CreatedAt,
        result.UpdatedAt,
    result.CheckedInAt,
        result.StartedAt,
        result.CompletedAt,
        result.CancelledAt,
        result.DidNotAttendAt
        );
  }

  [HttpPost]
  public async Task<ActionResult<AppointmentResponse>>
      CreateAppointment(
          [FromBody] CreateAppointmentRequest request,
          CancellationToken cancellationToken)
  {
    var command =
        new CreateAppointmentCommand(
            request.AppointmentReference,
            request.PatientId,
            request.ReferralId,
            request.AppointmentType,
            request.ScheduledStart,
            request.ScheduledEnd,
            request.Location);

    var result =
        await _createAppointmentService
            .ExecuteAsync(
                command,
                cancellationToken);

    var response = ToResponse(result);

    return Created(
        $"/api/appointments/{response.Id}",
        response);
  }

  [HttpPost("{id:guid}/check-in")]
  public async Task<ActionResult<AppointmentResponse>>
    CheckIn(
        Guid id,
        CancellationToken cancellationToken)
  {
    var appointment =
        await _checkInAppointmentService
            .ExecuteAsync(
                id,
                cancellationToken);

    return Ok(
        ToResponse(appointment));
  }

  [HttpPost("{id:guid}/start")]
  public async Task<ActionResult<AppointmentResponse>>
    Start(
        Guid id,
        CancellationToken cancellationToken)
  {
    var appointment =
        await _startAppointmentService
            .ExecuteAsync(
                id,
                cancellationToken);

    return Ok(
        ToResponse(appointment));
  }

  [HttpPost("{id:guid}/complete")]
  public async Task<ActionResult<AppointmentResponse>>
    Complete(
        Guid id,
        CancellationToken cancellationToken)
  {
    var appointment =
        await _completeAppointmentService
            .ExecuteAsync(
                id,
                cancellationToken);

    return Ok(
        ToResponse(appointment));
  }

  [HttpPost("{id:guid}/cancel")]
  public async Task<ActionResult<AppointmentResponse>>
    Cancel(
        Guid id,
        CancellationToken cancellationToken)
  {
    var appointment =
        await _cancelAppointmentService
            .ExecuteAsync(
                id,
                cancellationToken);

    return Ok(
        ToResponse(appointment));
  }

  [HttpPost("{id:guid}/did-not-attend")]
  public async Task<ActionResult<AppointmentResponse>>
    MarkDidNotAttend(
        Guid id,
        CancellationToken cancellationToken)
  {
    var appointment =
        await _didNotAttendService
            .ExecuteAsync(
                id,
                cancellationToken);

    return Ok(
        ToResponse(appointment));
  }
}