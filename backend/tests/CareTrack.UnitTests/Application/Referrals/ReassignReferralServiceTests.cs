using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.ReassignReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;


namespace CareTrack.UnitTests.Application.Referrals;

public class ReassignReferralServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenReferralIsAssigned_ReassignsReferral()
  {
    // Arrange
    var repository = new FakeReferralRepository();

    var referral = CreateAssignedReferral();

    var firstAssignedAt = referral.AssignedAt;

    await repository.AddAsync(referral);

    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    var command = new ReassignReferralCommand(
        referral.Id,
        "Cardiology Team B");

    // Act
    var result = await service.ExecuteAsync(command);

    // Assert
    Assert.Equal(
        ReferralStatus.Assigned,
        result.Status);

    Assert.Equal(
        "Cardiology Team B",
        result.AssignedTo);

    Assert.NotNull(result.AssignedAt);

    Assert.True(
        result.AssignedAt >= firstAssignedAt);

    Assert.Equal(
        1,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var repository = new FakeReferralRepository();

    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    var command = new ReassignReferralCommand(
        Guid.NewGuid(),
        "Cardiology Team B");

    // Act
    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(command));

    // Assert
    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralIsAcceptedButNotAssigned_ThrowsInvalidStateTransitionException()
  {
    // Arrange
    var repository = new FakeReferralRepository();

    var referral = CreateAcceptedReferral();

    await repository.AddAsync(referral);

    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    var command = new ReassignReferralCommand(
        referral.Id,
        "Cardiology Team B");

    // Act
    await Assert.ThrowsAsync<InvalidStateTransitionException>(
        () => service.ExecuteAsync(command));

    // Assert
    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);

    Assert.Null(referral.AssignedTo);
    Assert.Null(referral.AssignedAt);

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WithBlankAssignmentTarget_ThrowsArgumentException()
  {
    // Arrange
    var repository = new FakeReferralRepository();

    var referral = CreateAssignedReferral();

    var originalAssignedTo = referral.AssignedTo;
    var originalAssignedAt = referral.AssignedAt;

    await repository.AddAsync(referral);

    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    var command = new ReassignReferralCommand(
        referral.Id,
        "   ");

    // Act
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    // Assert
    Assert.Equal(
        originalAssignedTo,
        referral.AssignedTo);

    Assert.Equal(
        originalAssignedAt,
        referral.AssignedAt);

    Assert.Equal(
        ReferralStatus.Assigned,
        referral.Status);

    Assert.Equal(
        0,
        repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WithDifferentCase_StoresCanonicalTarget()
  {
    var repository = new FakeReferralRepository();
    var referral = CreateAssignedReferral();
    await repository.AddAsync(referral);
    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    var result = await service.ExecuteAsync(
        new ReassignReferralCommand(
            referral.Id,
            "  cardiology team b  "));

    Assert.Equal("Cardiology Team B", result.AssignedTo);
    Assert.Equal(1, repository.SaveChangesCallCount);
  }

  [Fact]
  public async Task ExecuteAsync_WithUnavailableTarget_ThrowsArgumentException()
  {
    var repository = new FakeReferralRepository();
    var referral = CreateAssignedReferral();
    var originalAssignedTo = referral.AssignedTo;
    await repository.AddAsync(referral);
    var service = new ReassignReferralService(
        repository,
        new FakeReferralAssignmentTargetDirectory(),
        NullLogger<ReassignReferralService>.Instance);

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new ReassignReferralCommand(
                referral.Id,
                "Unknown Team")));

    Assert.Equal(originalAssignedTo, referral.AssignedTo);
    Assert.Equal(0, repository.SaveChangesCallCount);
  }

  private static Referral CreateAcceptedReferral()
  {
    var referral = new Referral(
        "REF-REASSIGN-TEST",
        Guid.NewGuid(),
        ReferralPriority.Routine,
        "Referral reason.");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();

    return referral;
  }

  private static Referral CreateAssignedReferral()
  {
    var referral = CreateAcceptedReferral();

    referral.Assign(
        "Cardiology Team A");

    return referral;
  }


}