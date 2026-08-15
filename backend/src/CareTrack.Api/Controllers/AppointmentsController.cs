using CareTrack.Api.Contracts.Appointments;
using CareTrack.Application.Appointments.CreateAppointment;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentsController : ControllerBase
{
  private readonly CreateAppointmentService _createAppointmentService;

  public AppointmentsController(
      CreateAppointmentService createAppointmentService)
  {
    _createAppointmentService =
        createAppointmentService;
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

    var response =
        new AppointmentResponse(
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
            result.UpdatedAt);

    return Created(
        $"/api/appointments/{response.Id}",
        response);
  }
}