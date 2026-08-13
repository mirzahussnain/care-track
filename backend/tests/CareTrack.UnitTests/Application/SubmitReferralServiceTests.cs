using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.SubmitReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application;

public class SubmitReferralServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenDraft_SubmitsReferral()
  {
    // Arrange
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-SUB-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    await repository.AddAsync(referral);

    var service =
        new SubmitReferralService(
            repository,
            NullLogger<SubmitReferralService>.Instance);

    // Act
    var result =
        await service.ExecuteAsync(
            new SubmitReferralCommand(
                referral.Id));

    // Assert
    Assert.Equal(
        ReferralStatus.Submitted,
        result.Status);

    Assert.NotNull(
        result.SubmittedAt);

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
        new SubmitReferralService(
            repository,
            NullLogger<SubmitReferralService>.Instance);

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(
            new SubmitReferralCommand(
                Guid.NewGuid())));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAlreadySubmitted_ThrowsInvalidStateTransitionException()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-SUB-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();

    await repository.AddAsync(referral);

    var service =
        new SubmitReferralService(
            repository,
            NullLogger<SubmitReferralService>.Instance);

    await Assert.ThrowsAsync<
        InvalidStateTransitionException>(
            () => service.ExecuteAsync(
                new SubmitReferralCommand(
                    referral.Id)));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }
}