using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.AssignReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using CareTrack.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;


namespace CareTrack.UnitTests.Application;

public class AssignReferralTests
{

  [Fact]
  public async Task ExecuteAsync_WhenAccepted_AssignsReferral()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    await repository.AddAsync(
        referral);

    var service =
        new AssignReferralService(
            repository,
            NullLogger<AssignReferralService>.Instance);

    var result =
        await service.ExecuteAsync(
            new AssignReferralCommand(
                referral.Id,
                "Cardiology Team A"));

    Assert.Equal(
        ReferralStatus.Assigned,
        result.Status);

    Assert.Equal(
        "Cardiology Team A",
        result.AssignedTo);

    Assert.NotNull(
        result.AssignedAt);

    Assert.Equal(
        1,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WithUnknownReferral_ThrowsNotFoundException()
  {
    var repository =
        new FakeReferralRepository();

    var service =
        new AssignReferralService(
            repository,
            NullLogger<AssignReferralService>.Instance);

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(
            new AssignReferralCommand(
                Guid.NewGuid(),
                "Cardiology Team A")));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralNotAccepted_ThrowsInvalidStateTransitionException()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-ASSIGN-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    await repository.AddAsync(
        referral);

    var service =
        new AssignReferralService(
            repository,
            NullLogger<AssignReferralService>.Instance);

    await Assert.ThrowsAsync<
        InvalidStateTransitionException>(
            () => service.ExecuteAsync(
                new AssignReferralCommand(
                    referral.Id,
                    "Cardiology Team A")));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }


}
