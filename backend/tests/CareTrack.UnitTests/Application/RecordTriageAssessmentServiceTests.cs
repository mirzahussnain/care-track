using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.RecordTriageAssessment;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application;

public class RecordTriageAssessment
{

  [Fact]
  public async Task ExecuteAsync_WhenAwaitingTriage_RecordsAssessment()
  {
    // Arrange
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-TRIAGE-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();
    referral.StartTriage();

    await repository.AddAsync(
        referral);

    var service =
        new RecordTriageAssessmentService(
            repository,
            NullLogger<RecordTriageAssessmentService>.Instance);

    // Act
    var result =
        await service.ExecuteAsync(
            new RecordTriageAssessmentCommand(
                referral.Id,
                ReferralPriority.Urgent,
                "Symptoms worsening."));

    // Assert
    Assert.Equal(
        ReferralPriority.Urgent,
        result.Priority);

    Assert.Equal(
        "Symptoms worsening.",
        result.TriageNote);

    Assert.NotNull(
        result.TriagedAt);

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
        new RecordTriageAssessmentService(
            repository,
            NullLogger<RecordTriageAssessmentService>.Instance);

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(
            new RecordTriageAssessmentCommand(
                Guid.NewGuid(),
                ReferralPriority.Urgent,
                "Triage note.")));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralNotAwaitingTriage_ThrowsInvalidStateTransitionException()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-TRIAGE-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    await repository.AddAsync(
        referral);

    var service =
        new RecordTriageAssessmentService(
            repository,
            NullLogger<RecordTriageAssessmentService>.Instance);

    await Assert.ThrowsAsync<InvalidStateTransitionException>(
        () => service.ExecuteAsync(
            new RecordTriageAssessmentCommand(
                referral.Id,
                ReferralPriority.Urgent,
                "Escalate.")));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WithBlankNote_ThrowsArgumentException()
  {
    var repository =
        new FakeReferralRepository();

    var referral =
        new Referral(
            "REF-TRIAGE-003",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();
    referral.StartTriage();

    await repository.AddAsync(
        referral);

    var service =
        new RecordTriageAssessmentService(
            repository,
            NullLogger<RecordTriageAssessmentService>.Instance);

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new RecordTriageAssessmentCommand(
                referral.Id,
                ReferralPriority.Urgent,
                "   ")));

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

}