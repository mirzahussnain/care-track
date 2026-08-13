using CareTrack.Api.Contracts.Referrals;
using CareTrack.Api.Mappings;
using CareTrack.Application.Referrals.CreateReferral;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/referrals")]
public sealed class ReferralsController
    : ControllerBase
{
  private readonly CreateReferralService
      _createReferralService;

  public ReferralsController(
      CreateReferralService createReferralService)
  {
    _createReferralService =
        createReferralService;
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
}