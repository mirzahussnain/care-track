using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.AssignReferral;
using CareTrack.Application.Referrals.GetReferralById;
using CareTrack.Application.Referrals.SearchReferrals;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using CareTrack.UnitTests.TestSupport.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.Referrals;

public class SearchReferralsTests
{
  private static SearchReferralsService CreateService()
  {
    var repository =
        new FakeReferralRepository();

    return new SearchReferralsService(
        repository);
  }

  private static SearchReferralsCommand CreateValidQuery()
  {
    return new SearchReferralsCommand(
        Status: null,
        Priority: null,
        PatientId: null,
        AssignedTo: null,
        CreatedFrom: null,
        CreatedTo: null,
        Page: 1,
        PageSize: 20,
        SortBy: "createdat",
        SortDirection: "desc");
  }

  [Fact]
  public async Task ExecuteAsync_WithPageLessThanOne_ThrowsArgumentException()
  {
    var service =
        CreateService();

    var query =
        CreateValidQuery()
        with
        {
          Page = 0
        };

    await Assert.ThrowsAsync<ArgumentException>(
        () =>
            service.ExecuteAsync(query));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(101)]
  public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentException(
    int pageSize)
  {
    var service =
        CreateService();

    var query =
        CreateValidQuery()
        with
        {
          PageSize = pageSize
        };

    await Assert.ThrowsAsync<ArgumentException>(
        () =>
            service.ExecuteAsync(query));
  }

  [Fact]
  public async Task ExecuteAsync_WhenCreatedFromIsAfterCreatedTo_ThrowsArgumentException()
  {
    var service =
        CreateService();

    var query =
        CreateValidQuery()
        with
        {
          CreatedFrom =
                new DateOnly(2026, 8, 15),

          CreatedTo =
                new DateOnly(2026, 8, 10)
        };

    await Assert.ThrowsAsync<ArgumentException>(
        () =>
            service.ExecuteAsync(query));
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralExists_ReturnsReferral()
  {
    var repository =
        new FakeReferralRepository();

    var referral = ReferralTestHelpers.CreateNewReferral();

    await repository.AddAsync(
        referral);

    var service =
        new GetReferralByIdService(
            repository);

    var result =
        await service.ExecuteAsync(
            new GetReferralByIdCommand(
                referral.Id));

    Assert.Equal(
        referral.Id,
        result.Id);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    var repository =
        new FakeReferralRepository();

    var service =
        new GetReferralByIdService(
            repository);

    await Assert.ThrowsAsync<NotFoundException>(
        () =>
            service.ExecuteAsync(
                new GetReferralByIdCommand(
                    Guid.NewGuid())));
  }







}