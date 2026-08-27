using System.Security.Claims;
using CareTrack.Api.Authorization;
using CareTrack.Api.Contracts.CurrentUser;
using CareTrack.Api.Identity;
using CareTrack.Application.Common.Interfaces;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CareTrack.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize(Policy = CareTrackPolicies.ApiAccess)]
public sealed class CurrentUserController(
    ICurrentUser currentUser,
    IDemoAccountDirectory demoAccountDirectory)
    : ControllerBase
{
  [HttpGet]
  public ActionResult<CurrentUserResponse> Get()
  {
    var username =
        User.FindFirstValue("preferred_username")
        ?? User.FindFirstValue(ClaimTypes.Email)
        ?? string.Empty;

    var name =
        User.FindFirstValue("name")
        ?? User.Identity?.Name
        ?? username;

    var roles =
        User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                role => role,
                StringComparer.Ordinal)
            .ToArray();

    return Ok(
        new CurrentUserResponse(
            currentUser.UserId,
            name,
            username,
            roles,
            demoAccountDirectory.Contains(currentUser.UserId)));
  }
}