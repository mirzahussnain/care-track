using CareTrack.Api.Contracts.Referrals;
using CareTrack.Api.Mappings;
using CareTrack.Application.Referrals.AcceptReferral;
using CareTrack.Application.Referrals.CreateReferral;
using CareTrack.Application.Referrals.RejectReferral;
using CareTrack.Application.Referrals.RequestMoreInformation;
using CareTrack.Application.Referrals.ResubmitReferral;
using CareTrack.Application.Referrals.StartTriage;
using CareTrack.Application.Referrals.SubmitReferral;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/referrals")]
public sealed class ReferralsController
    : ControllerBase
{
  private readonly CreateReferralService _createReferralService;
  private readonly SubmitReferralService _submitReferralService;
  private readonly AcceptReferralService _acceptRefferalService;

  private readonly StartTriageService _startTriageService;

  private readonly RejectReferralService _rejectReferralService;

  private readonly RequestMoreInformationService _requestMoreInformationService;

  private readonly ResubmitReferralService _resubmitReferralService;
  public ReferralsController(
      CreateReferralService createReferralService,
      SubmitReferralService submitReferralService,
      AcceptReferralService acceptRefferalService,
      StartTriageService startTriageService,
      RejectReferralService rejectReferralService,
      RequestMoreInformationService requestMoreInformationService,
      ResubmitReferralService resubmitReferralService)
  {
    _createReferralService = createReferralService;
    _submitReferralService = submitReferralService;
    _acceptRefferalService = acceptRefferalService;
    _startTriageService = startTriageService;
    _rejectReferralService = rejectReferralService;
    _requestMoreInformationService = requestMoreInformationService;
    _resubmitReferralService = resubmitReferralService;
  }

  [HttpPost]
  public async Task<ActionResult<ReferralResponse>>
      CreateReferral(
          CreateReferralRequest request,
          CancellationToken cancellationToken)
  {
    var command =
        new CreateReferralCommand(
            request.ReferralReference,
            request.PatientId,
            request.Priority,
            request.Reason);

    var referral =
        await _createReferralService.ExecuteAsync(
            command,
            cancellationToken);

    return Created(
        $"/api/referrals/{referral.Id}",
        referral.ToResponse());
  }
  [HttpPost("{id:guid}/submit")]
  public async Task<ActionResult<ReferralResponse>>
    SubmitReferral(
        Guid id,
        CancellationToken cancellationToken)
  {
    var referral =
        await _submitReferralService.ExecuteAsync(
            new SubmitReferralCommand(id),
            cancellationToken);

    return Ok(
        referral.ToResponse());
  }
  [HttpPost("{id:guid}/start-triage")]
  public async Task<ActionResult<ReferralResponse>>
  StartTriage(
      Guid id,
      CancellationToken cancellationToken)
  {
    var referral =
        await _startTriageService.ExecuteAsync(
            new StartTriageCommand(id),
            cancellationToken);

    return Ok(
        referral.ToResponse());
  }

  [HttpPost("{id:guid}/accept")]
  public async Task<ActionResult<ReferralResponse>> AcceptReferral(Guid id, CancellationToken cancellationToken)
  {
    var referral =
    await _acceptRefferalService.ExecuteAsync(
        new AcceptReferralCommand(id),
        cancellationToken);
    return Ok(
   referral.ToResponse());
  }

  [HttpPost("{id:guid}/request-more-information")]
  public async Task<ActionResult<ReferralResponse>> RequestMoreInformation(Guid id, CancellationToken cancellationToken)
  {
    var referral =
    await _requestMoreInformationService.ExecuteAsync(
        new RequestMoreInformationCommand(id),
        cancellationToken);
    return Ok(
   referral.ToResponse());
  }
  [HttpPost("{id:guid}/reject")]
  public async Task<ActionResult<ReferralResponse>> RejectReferral(Guid id, CancellationToken cancellationToken)
  {
    var referral =
    await _rejectReferralService.ExecuteAsync(
        new RejectReferralCommand(id),
        cancellationToken);
    return Ok(
   referral.ToResponse());
  }
  [HttpPost("{id:guid}/resubmit")]
  public async Task<ActionResult<ReferralResponse>> ResubmitReferral(Guid id, CancellationToken cancellationToken)
  {
    var referral =
    await _resubmitReferralService.ExecuteAsync(
        new ResubmitReferralCommand(id),
        cancellationToken);
    return Ok(
   referral.ToResponse());
  }
}