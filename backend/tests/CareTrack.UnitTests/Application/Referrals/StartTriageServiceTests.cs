using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.StartTriage;
using CareTrack.Application.Referrals.SubmitReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.Referrals;

public class StartTriageServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenSubmitted_StartsTriage()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-TRI-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();

    await repository.AddAsync(referral);

    var service =
        new StartTriageService(
            repository,
            NullLogger<StartTriageService>.Instance);

    var result =
        await service.ExecuteAsync(
            new StartTriageCommand(
                referral.Id));

    Assert.Equal(
        ReferralStatus.AwaitingTriage,
        result.Status);

    Assert.Equal(
        1,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenDraft_ThrowsInvalidStateTransitionException()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-TRI-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    await repository.AddAsync(referral);

    var service =
        new StartTriageService(
            repository,
            NullLogger<StartTriageService>.Instance);

    await Assert.ThrowsAsync<
        InvalidStateTransitionException>(
            () => service.ExecuteAsync(
                new StartTriageCommand(
                    referral.Id)));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }
}