using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;

namespace CareTrack.Application.Referrals.SearchReferrals;

public sealed class SearchReferralsService
{
  private readonly IReferralRepository
      _referralRepository;

  public SearchReferralsService(
      IReferralRepository referralRepository)
  {
    _referralRepository =
        referralRepository;
  }

  public async Task<PagedResult<Referral>>
      ExecuteAsync(
          SearchReferralsCommand command,
          CancellationToken cancellationToken = default)
  {
    Validate(command);

    return await _referralRepository.SearchAsync(
        command.Status,
    command.Priority,
    command.PatientId,
    command.AssignedTo,
    command.CreatedFrom,
    command.CreatedTo,
    command.Page,
    command.PageSize,
    command.SortBy,
    command.SortDirection,
     cancellationToken);
  }

  private static void Validate(
      SearchReferralsCommand command)
  {
    if (command.Page < 1)
    {
      throw new ArgumentException(
          "Page must be at least 1.");
    }

    if (command.PageSize < 1 ||
        command.PageSize > 100)
    {
      throw new ArgumentException(
          "Page size must be between 1 and 100.");
    }

    if (command.CreatedFrom.HasValue &&
        command.CreatedTo.HasValue &&
        command.CreatedFrom >
        command.CreatedTo)
    {
      throw new ArgumentException(
          "CreatedFrom cannot be later than CreatedTo.");
    }
    var validDirection =
      command.SortDirection.Equals(
          "asc",
          StringComparison.OrdinalIgnoreCase)
      ||
      command.SortDirection.Equals(
          "desc",
          StringComparison.OrdinalIgnoreCase);

    if (!validDirection)
    {
      throw new ArgumentException(
          "Sort direction must be 'asc' or 'desc'.");
    }

    var validSortFields = new[]
    {
        "createdat",
        "updatedat",
        "priority",
        "status",
        "referralreference"
    };

    if (!validSortFields.Contains(
        command.SortBy,
        StringComparer.OrdinalIgnoreCase))
    {
      throw new ArgumentException(
          "Unsupported referral sort field.");
    }
  }
}