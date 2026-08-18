using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.GetReferralHistory;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using CareTrack.UnitTests.TestSupport.Helpers;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.Referrals;

public class GetReferralHistoryTests
{
  [Fact]
  public async Task ExecuteAsync_WhenReferralExists_ReturnsHistory()
  {
    // Arrange
    var repository =
        new FakeReferralRepository();

    var referral = ReferralTestHelpers.CreateAcceptedReferral();

    await repository.AddAsync(
        referral);

    var service =
        new GetReferralHistoryService(
            repository);

    // Act
    var result =
        await service.ExecuteAsync(
            new GetReferralHistoryCommand(
                referral.Id));

    // Assert
    Assert.NotEmpty(
        result);

    Assert.Equal(
        ReferralHistoryEventType.Created,
        result.First().EventType);

    Assert.Equal(
        ReferralHistoryEventType.Accepted,
        result.Last().EventType);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var repository =
        new FakeReferralRepository();

    var service =
        new GetReferralHistoryService(
            repository);

    // Act / Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () =>
            service.ExecuteAsync(
                new GetReferralHistoryCommand(
                    Guid.NewGuid())));
  }

}