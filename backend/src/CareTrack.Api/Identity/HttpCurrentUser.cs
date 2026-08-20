using CareTrack.Application.Common.Interfaces;
using Microsoft.Identity.Web;

namespace CareTrack.Api.Identity;

public sealed class HttpCurrentUser(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUser
{
  public string UserId
  {
    get
    {
      var user =
          httpContextAccessor.HttpContext?.User
          ?? throw new InvalidOperationException(
              "No current HTTP user is available.");

      if (user.Identity?.IsAuthenticated != true)
      {
        throw new InvalidOperationException(
            "The current user is not authenticated.");
      }

      var userId =
          user.FindFirst(ClaimConstants.Oid)?.Value ?? user.FindFirst(ClaimConstants.ObjectId)?.Value;

      if (string.IsNullOrWhiteSpace(userId))
      {
        throw new InvalidOperationException(
            "The authenticated user does not contain an object ID claim.");
      }

      return userId;
    }
  }
}