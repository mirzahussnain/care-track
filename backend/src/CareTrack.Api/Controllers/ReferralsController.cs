using CareTrack.Api.Contracts.Referrals;
using CareTrack.Api.Mappings;
using CareTrack.Application.Referrals.AcceptReferral;
using CareTrack.Application.Referrals.AssignReferral;
using CareTrack.Application.Referrals.CompleteReferral;
using CareTrack.Application.Referrals.CreateReferral;
using CareTrack.Application.Referrals.GetReferralById;
using CareTrack.Application.Referrals.GetReferralHistory;
using CareTrack.Application.Referrals.ReassignReferral;
using CareTrack.Application.Referrals.RecordTriageAssessment;
using CareTrack.Application.Referrals.RejectReferral;
using CareTrack.Application.Referrals.RequestMoreInformation;
using CareTrack.Application.Referrals.ResubmitReferral;
using CareTrack.Application.Referrals.SearchReferrals;
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

  private readonly RecordTriageAssessmentService _recordTriageAssessmentService;

  private readonly AssignReferralService _assignReferralService;


  private readonly ReassignReferralService _reassignReferralService;

  private readonly CompleteReferralService _completeReferralService;
  private readonly GetReferralHistoryService _getReferralHistoryService;


  private readonly GetReferralByIdService _getReferralByIdService;
  private readonly SearchReferralsService _searchReferralsService;

  public ReferralsController(
      CreateReferralService createReferralService,
      SubmitReferralService submitReferralService,
      AcceptReferralService acceptRefferalService,
      StartTriageService startTriageService,
      RejectReferralService rejectReferralService,
      RequestMoreInformationService requestMoreInformationService,
      ResubmitReferralService resubmitReferralService,
      RecordTriageAssessmentService recordTriageAssessmentService,
      AssignReferralService assignReferralService,
      ReassignReferralService reassignReferralSerivce,
      GetReferralHistoryService getReferralHistoryService,
      GetReferralByIdService getReferralByIdService,
      SearchReferralsService searchReferralsService,
      CompleteReferralService completeReferralService)
  {
    _createReferralService = createReferralService;
    _submitReferralService = submitReferralService;
    _acceptRefferalService = acceptRefferalService;
    _startTriageService = startTriageService;
    _rejectReferralService = rejectReferralService;
    _requestMoreInformationService = requestMoreInformationService;
    _resubmitReferralService = resubmitReferralService;
    _recordTriageAssessmentService = recordTriageAssessmentService;
    _assignReferralService = assignReferralService;
    _reassignReferralService = reassignReferralSerivce;
    _getReferralHistoryService = getReferralHistoryService;
    _getReferralByIdService = getReferralByIdService;
    _searchReferralsService = searchReferralsService;
    _completeReferralService = completeReferralService;
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

  [HttpPost("{id:guid}/triage-assessment")]
  public async Task<ActionResult<ReferralResponse>>
    RecordTriageAssessment(
        Guid id,
        RecordTriageAssessmentRequest request,
        CancellationToken cancellationToken)
  {
    var referral =
        await _recordTriageAssessmentService.ExecuteAsync(
            new RecordTriageAssessmentCommand(
                id,
                request.Priority,
                request.Note),
            cancellationToken);

    return Ok(
        referral.ToResponse());
  }

  [HttpPost("{id:guid}/assign")]
  public async Task<ActionResult<ReferralResponse>>
    AssignReferral(
        Guid id,
        AssignReferralRequest request,
        CancellationToken cancellationToken)
  {
    var referral =
        await _assignReferralService.ExecuteAsync(
            new AssignReferralCommand(
                id,
                request.AssignedTo),
            cancellationToken);

    return Ok(
        referral.ToResponse());
  }

  [HttpPost("{id:guid}/reassign")]
  public async Task<ActionResult<ReferralResponse>>
    ReassignReferral(
        Guid id,
        AssignReferralRequest request,
        CancellationToken cancellationToken)
  {
    var referral =
        await _reassignReferralService.ExecuteAsync(
            new ReassignReferralCommand(
                id,
                request.AssignedTo),
            cancellationToken);

    return Ok(
        referral.ToResponse());
  }

  [HttpPost("{id:guid}/complete")]
  public async Task<IActionResult> Complete(
    Guid id,
    CancellationToken cancellationToken)
  {
    await _completeReferralService.ExecuteAsync(
        new CompleteReferralCommand(id),
        cancellationToken);

    return NoContent();


  }

  [HttpGet("{id:guid}/history")]
  public async Task<
    ActionResult<IReadOnlyList<ReferralHistoryResponse>>>
    GetHistory(
        Guid id,
        CancellationToken cancellationToken)
  {
    var history = await _getReferralHistoryService
            .ExecuteAsync(
                new GetReferralHistoryCommand(id),
                cancellationToken);

    return Ok(
        history
            .Select(
                entry =>
                    entry.ToResponse())
            .ToList());
  }

  [HttpGet]
  public async Task<
    ActionResult<
        PagedReferralResponse>>
    SearchReferrals(
        [FromQuery]
        SearchReferralRequest request,
        CancellationToken cancellationToken)
  {
    var result =
        await _searchReferralsService
            .ExecuteAsync(
                new SearchReferralsCommand(
                    request.Status,
                    request.Priority,
                    request.PatientId,
                    request.AssignedTo,
                    request.CreatedFrom,
                    request.CreatedTo,
                    request.SortBy,
                    request.SortDirection,
                    request.Page,
                    request.PageSize),
                cancellationToken);

    return Ok(
        new PagedReferralResponse(
            result.Items
                .Select(
                    referral =>
                        referral.ToResponse())
                .ToList(),
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages));
  }

  [HttpGet("{id:guid}")]
  public async Task<ActionResult<ReferralResponse>>
    GetReferralById(
        Guid id,
        CancellationToken cancellationToken)
  {
    var referral =
        await _getReferralByIdService
            .ExecuteAsync(
                new GetReferralByIdCommand(id),
                cancellationToken);

    return Ok(
        referral.ToResponse());
  }
}