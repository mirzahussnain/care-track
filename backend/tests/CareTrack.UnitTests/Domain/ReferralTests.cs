using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Helpers;

namespace CareTrack.UnitTests.Domain;

public class ReferralTests
{
    // =========================================================
    // Constructor
    // =========================================================

    [Fact]
    public void Constructor_WithValidValues_CreatesDraftReferral()
    {
        // Arrange
        var patientId = Guid.NewGuid();

        // Act
        var referral =
            new Referral(
                "REF-001",
                patientId,
                ReferralPriority.Routine,
                "Persistent shoulder pain.");

        // Assert
        Assert.NotEqual(
            Guid.Empty,
            referral.Id);

        Assert.Equal(
            "REF-001",
            referral.ReferralReference);

        Assert.Equal(
            patientId,
            referral.PatientId);

        Assert.Equal(
            ReferralPriority.Routine,
            referral.Priority);

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);

        Assert.Equal(
            "Persistent shoulder pain.",
            referral.Reason);

        Assert.Null(
            referral.SubmittedAt);
    }

    [Fact]
    public void Constructor_TrimsReferenceAndReason()
    {
        // Arrange / Act
        var referral =
            new Referral(
                "  REF-001  ",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "  Shoulder pain  ");

        // Assert
        Assert.Equal(
            "REF-001",
            referral.ReferralReference);

        Assert.Equal(
            "Shoulder pain",
            referral.Reason);
    }

    [Fact]
    public void Constructor_WithBlankReference_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new Referral(
                "   ",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Reason"));
    }

    [Fact]
    public void Constructor_WithEmptyPatientId_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new Referral(
                "REF-001",
                Guid.Empty,
                ReferralPriority.Routine,
                "Reason"));
    }

    [Fact]
    public void Constructor_WithBlankReason_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () => new Referral(
                "REF-001",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "   "));
    }

    // =========================================================
    // Submit
    // =========================================================

    [Fact]
    public void Submit_WhenDraft_ChangesStatusToSubmitted()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act
        referral.Submit();

        // Assert
        Assert.Equal(
            ReferralStatus.Submitted,
            referral.Status);

        Assert.NotNull(
            referral.SubmittedAt);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        referral.Submit();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.Submit());
    }

    // =========================================================
    // Triage workflow
    // =========================================================

    [Fact]
    public void StartTriage_WhenSubmitted_ChangesStatusToAwaitingTriage()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        referral.Submit();

        var previousUpdatedAt =
            referral.UpdatedAt;

        // Act
        referral.StartTriage();

        // Assert
        Assert.Equal(
            ReferralStatus.AwaitingTriage,
            referral.Status);

        Assert.NotNull(
            referral.UpdatedAt);

        Assert.True(
            referral.UpdatedAt >= previousUpdatedAt);
    }

    [Fact]
    public void StartTriage_WhenDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.StartTriage());

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);
    }

    [Fact]
    public void StartTriage_WhenAccepted_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        referral.Accept();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.StartTriage());

        Assert.Equal(
            ReferralStatus.Accepted,
            referral.Status);
    }

    // =========================================================
    // Accept / Reject
    // =========================================================

    [Fact]
    public void Accept_WhenAwaitingTriage_ChangesStatusToAccepted()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act
        referral.Accept();

        // Assert
        Assert.Equal(
            ReferralStatus.Accepted,
            referral.Status);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void Accept_WhenSubmitted_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        referral.Submit();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.Accept());

        Assert.Equal(
            ReferralStatus.Submitted,
            referral.Status);
    }

    [Fact]
    public void Reject_WhenAwaitingTriage_ChangesStatusToRejected()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act
        referral.Reject();

        // Assert
        Assert.Equal(
            ReferralStatus.Rejected,
            referral.Status);
    }

    // =========================================================
    // More information / resubmit
    // =========================================================

    [Fact]
    public void RequestMoreInformation_WhenAwaitingTriage_ChangesStatus()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act
        referral.RequestMoreInformation();

        // Assert
        Assert.Equal(
            ReferralStatus.MoreInformationRequired,
            referral.Status);
    }

    [Fact]
    public void Resubmit_WhenMoreInformationRequired_ReturnsToSubmitted()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        referral.RequestMoreInformation();

        var originalSubmittedAt =
            referral.SubmittedAt;

        // Act
        referral.Resubmit();

        // Assert
        Assert.Equal(
            ReferralStatus.Submitted,
            referral.Status);

        Assert.Equal(
            originalSubmittedAt,
            referral.SubmittedAt);
    }

    [Fact]
    public void Resubmit_WhenDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.Resubmit());

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);
    }

    // =========================================================
    // Triage assessment
    // =========================================================

    [Fact]
    public void RecordTriageAssessment_WhenAwaitingTriage_UpdatesPriorityAndTriageData()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act
        referral.RecordTriageAssessment(
            ReferralPriority.Urgent,
            "Symptoms have worsened.");

        // Assert
        Assert.Equal(
            ReferralPriority.Urgent,
            referral.Priority);

        Assert.Equal(
            "Symptoms have worsened.",
            referral.TriageNote);

        Assert.NotNull(
            referral.TriagedAt);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void RecordTriageAssessment_WhenDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.RecordTriageAssessment(
                ReferralPriority.Urgent,
                "Escalated during triage."));

        Assert.Equal(
            ReferralPriority.Routine,
            referral.Priority);

        Assert.Null(
            referral.TriageNote);

        Assert.Null(
            referral.TriagedAt);
    }

    [Fact]
    public void RecordTriageAssessment_WithBlankNote_ThrowsArgumentException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act / Assert
        Assert.Throws<ArgumentException>(
            () => referral.RecordTriageAssessment(
                ReferralPriority.Urgent,
                "   "));

        Assert.Null(
            referral.TriageNote);
    }

    [Fact]
    public void RecordTriageAssessment_WithNoteLongerThan2000Characters_ThrowsArgumentException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        var longNote =
            new string('A', 2001);

        // Act / Assert
        Assert.Throws<ArgumentException>(
            () => referral.RecordTriageAssessment(
                ReferralPriority.Urgent,
                longNote));
    }

    [Fact]
    public void RecordTriageAssessment_WithInvalidPriority_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        var invalidPriority =
            (ReferralPriority)999;

        // Act / Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => referral.RecordTriageAssessment(
                invalidPriority,
                "Triage note."));
    }

    [Fact]
    public void RecordTriageAssessment_TrimsTriageNote()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        // Act
        referral.RecordTriageAssessment(
            ReferralPriority.Routine,
            "  Suitable for routine review.  ");

        // Assert
        Assert.Equal(
            "Suitable for routine review.",
            referral.TriageNote);
    }

    [Fact]
    public void RecordTriageAssessment_WhenCalledAgain_ReplacesCurrentTriageAssessment()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAwaitingTriageReferral();

        referral.RecordTriageAssessment(
            ReferralPriority.Routine,
            "Initially suitable for routine review.");

        var firstTriagedAt =
            referral.TriagedAt;

        // Act
        referral.RecordTriageAssessment(
            ReferralPriority.Urgent,
            "Condition deteriorated.");

        // Assert
        Assert.Equal(
            ReferralPriority.Urgent,
            referral.Priority);

        Assert.Equal(
            "Condition deteriorated.",
            referral.TriageNote);

        Assert.True(
            referral.TriagedAt >= firstTriagedAt);
    }

    // =========================================================
    // Assignment
    // =========================================================

    [Fact]
    public void Assign_WhenAccepted_AssignsReferral()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        // Act
        referral.Assign(
            "Cardiology Team A");

        // Assert
        Assert.Equal(
            ReferralStatus.Assigned,
            referral.Status);

        Assert.Equal(
            "Cardiology Team A",
            referral.AssignedTo);

        Assert.NotNull(
            referral.AssignedAt);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void Assign_TrimsAssignmentTarget()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        // Act
        referral.Assign(
            "  Cardiology Team A  ");

        // Assert
        Assert.Equal(
            "Cardiology Team A",
            referral.AssignedTo);
    }

    [Fact]
    public void Assign_WhenDraft_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.Assign(
                "Cardiology Team A"));

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);

        Assert.Null(
            referral.AssignedTo);

        Assert.Null(
            referral.AssignedAt);
    }

    [Fact]
    public void Assign_WithBlankTarget_ThrowsArgumentException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        // Act / Assert
        Assert.Throws<ArgumentException>(
            () => referral.Assign(
                "   "));

        Assert.Equal(
            ReferralStatus.Accepted,
            referral.Status);

        Assert.Null(
            referral.AssignedTo);
    }

    [Fact]
    public void Assign_WithTargetLongerThan200Characters_ThrowsArgumentException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        var target =
            new string('A', 201);

        // Act / Assert
        Assert.Throws<ArgumentException>(
            () => referral.Assign(target));

        Assert.Equal(
            ReferralStatus.Accepted,
            referral.Status);
    }

    [Fact]
    public void Reassign_WhenAssigned_UpdatesAssignment()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        referral.Assign(
            "Cardiology Team A");

        var firstAssignedAt =
            referral.AssignedAt;

        // Act
        referral.Reassign(
            "Cardiology Team B");

        // Assert
        Assert.Equal(
            ReferralStatus.Assigned,
            referral.Status);

        Assert.Equal(
            "Cardiology Team B",
            referral.AssignedTo);

        Assert.NotNull(
            referral.AssignedAt);

        Assert.True(
            referral.AssignedAt >= firstAssignedAt);
    }

    [Fact]
    public void Reassign_WhenAccepted_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        // Act / Assert
        Assert.Throws<InvalidOperationException>(
            () => referral.Reassign(
                "Cardiology Team B"));

        Assert.Equal(
            ReferralStatus.Accepted,
            referral.Status);

        Assert.Null(
            referral.AssignedTo);
    }

    // =========================================================
    // Existing referral history
    // =========================================================

    [Fact]
    public void Constructor_CreatesCreatedHistoryEntry()
    {
        // Arrange / Act
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Assert
        var history =
            Assert.Single(
                referral.History);

        Assert.Equal(
            ReferralHistoryEventType.Created,
            history.EventType);

        Assert.Null(
            history.FromStatus);

        Assert.Equal(
            ReferralStatus.Draft,
            history.ToStatus);

        Assert.Equal(
            ReferralPriority.Routine,
            history.Priority);

        Assert.Equal(
            referral.CreatedAt,
            history.OccurredAt);
    }

    [Fact]
    public void Submit_AddsSubmittedHistoryEntry()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        // Act
        referral.Submit();

        // Assert
        var history =
            referral.History.Last();

        Assert.Equal(
            ReferralHistoryEventType.Submitted,
            history.EventType);

        Assert.Equal(
            ReferralStatus.Draft,
            history.FromStatus);

        Assert.Equal(
            ReferralStatus.Submitted,
            history.ToStatus);

        Assert.Equal(
            referral.UpdatedAt,
            history.OccurredAt);
    }

    [Fact]
    public void RecordTriageAssessment_AddsHistoryWithAssessmentData()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        referral.Submit();
        referral.StartTriage();

        // Act
        referral.RecordTriageAssessment(
            ReferralPriority.Urgent,
            "Symptoms have worsened.");

        // Assert
        var history =
            referral.History.Last();

        Assert.Equal(
            ReferralHistoryEventType.TriageAssessmentRecorded,
            history.EventType);

        Assert.Equal(
            ReferralStatus.AwaitingTriage,
            history.FromStatus);

        Assert.Equal(
            ReferralStatus.AwaitingTriage,
            history.ToStatus);

        Assert.Equal(
            ReferralPriority.Urgent,
            history.Priority);

        Assert.Equal(
            "Symptoms have worsened.",
            history.TriageNote);

        Assert.Equal(
            referral.TriagedAt,
            history.OccurredAt);
    }

    [Fact]
    public void RecordTriageAssessment_WhenReassessed_PreservesBothHistoryEntries()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        referral.Submit();
        referral.StartTriage();

        // Act
        referral.RecordTriageAssessment(
            ReferralPriority.Routine,
            "Routine review.");

        referral.RecordTriageAssessment(
            ReferralPriority.Urgent,
            "Condition worsened.");

        // Assert
        var assessments =
            referral.History
                .Where(
                    history =>
                        history.EventType ==
                        ReferralHistoryEventType
                            .TriageAssessmentRecorded)
                .ToList();

        Assert.Equal(
            2,
            assessments.Count);

        Assert.Equal(
            ReferralPriority.Routine,
            assessments[0].Priority);

        Assert.Equal(
            "Routine review.",
            assessments[0].TriageNote);

        Assert.Equal(
            ReferralPriority.Urgent,
            assessments[1].Priority);

        Assert.Equal(
            "Condition worsened.",
            assessments[1].TriageNote);
    }

    [Fact]
    public void Assign_AddsAssignmentHistory()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        // Act
        referral.Assign(
            "Cardiology Team A");

        // Assert
        var history =
            referral.History.Last();

        Assert.Equal(
            ReferralHistoryEventType.Assigned,
            history.EventType);

        Assert.Equal(
            ReferralStatus.Accepted,
            history.FromStatus);

        Assert.Equal(
            ReferralStatus.Assigned,
            history.ToStatus);

        Assert.Equal(
            "Cardiology Team A",
            history.AssignedTo);
    }

    [Fact]
    public void Reassign_PreservesPreviousAssignmentHistory()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateAcceptedReferral();

        referral.Assign(
            "Cardiology Team A");

        // Act
        referral.Reassign(
            "Cardiology Team B");

        // Assert
        var assignmentHistory =
            referral.History
                .Where(
                    history =>
                        history.EventType ==
                        ReferralHistoryEventType.Assigned
                        ||
                        history.EventType ==
                        ReferralHistoryEventType.Reassigned)
                .ToList();

        Assert.Equal(
            2,
            assignmentHistory.Count);

        Assert.Equal(
            "Cardiology Team A",
            assignmentHistory[0].AssignedTo);

        Assert.Equal(
            "Cardiology Team B",
            assignmentHistory[1].AssignedTo);
    }

    [Fact]
    public void Assign_WhenReferralIsDraft_DoesNotAddHistory()
    {
        // Arrange
        var referral =
            ReferralTestHelpers.CreateNewReferral();

        var historyCountBefore =
            referral.History.Count;

        // Act
        Assert.Throws<InvalidOperationException>(
            () => referral.Assign(
                "Cardiology Team A"));

        // Assert
        Assert.Equal(
            historyCountBefore,
            referral.History.Count);
    }

    // =========================================================
    // 4G - Schedule
    // =========================================================

    [Fact]
    public void Schedule_WhenReferralIsAssigned_SetsStatusToScheduled()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-001",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        // Act
        referral.Schedule();

        // Assert
        Assert.Equal(
            ReferralStatus.Scheduled,
            referral.Status);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void Schedule_WhenReferralIsNotAssigned_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-002",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        var historyCountBefore =
            referral.History.Count;

        // Act
        var action =
            () => referral.Schedule();

        // Assert
        Assert.Throws<InvalidOperationException>(
            action);

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);

        Assert.Equal(
            historyCountBefore,
            referral.History.Count);
    }

    [Fact]
    public void Schedule_AddsScheduledHistoryEntry()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-003",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        // Act
        referral.Schedule();

        // Assert
        var historyEntry =
            Assert.Single(
                referral.History,
                entry =>
                    entry.EventType ==
                    ReferralHistoryEventType.Scheduled);

        Assert.Equal(
            ReferralStatus.Assigned,
            historyEntry.FromStatus);

        Assert.Equal(
            ReferralStatus.Scheduled,
            historyEntry.ToStatus);

        Assert.Equal(
            referral.UpdatedAt,
            historyEntry.OccurredAt);
    }

    // =========================================================
    // 4G - Start progress
    // =========================================================

    [Fact]
    public void StartProgress_WhenReferralIsScheduled_SetsStatusToInProgress()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-004",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();

        // Act
        referral.StartProgress();

        // Assert
        Assert.Equal(
            ReferralStatus.InProgress,
            referral.Status);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void StartProgress_WhenReferralIsNotScheduled_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-005",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        var historyCountBefore =
            referral.History.Count;

        // Act
        var action =
            () => referral.StartProgress();

        // Assert
        Assert.Throws<InvalidOperationException>(
            action);

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);

        Assert.Equal(
            historyCountBefore,
            referral.History.Count);
    }

    [Fact]
    public void StartProgress_AddsStartedHistoryEntry()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-006",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();

        // Act
        referral.StartProgress();

        // Assert
        var historyEntry =
            Assert.Single(
                referral.History,
                entry =>
                    entry.EventType ==
                    ReferralHistoryEventType.Started);

        Assert.Equal(
            ReferralStatus.Scheduled,
            historyEntry.FromStatus);

        Assert.Equal(
            ReferralStatus.InProgress,
            historyEntry.ToStatus);

        Assert.Equal(
            referral.UpdatedAt,
            historyEntry.OccurredAt);
    }

    // =========================================================
    // 4G - Complete
    // =========================================================

    [Fact]
    public void Complete_WhenReferralIsInProgress_SetsStatusToCompleted()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-007",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();
        referral.StartProgress();

        // Act
        referral.Complete();

        // Assert
        Assert.Equal(
            ReferralStatus.Completed,
            referral.Status);

        Assert.NotNull(
            referral.UpdatedAt);
    }

    [Fact]
    public void Complete_WhenReferralIsNotInProgress_ThrowsInvalidOperationException()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-008",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        var historyCountBefore =
            referral.History.Count;

        // Act
        var action =
            () => referral.Complete();

        // Assert
        Assert.Throws<InvalidOperationException>(
            action);

        Assert.Equal(
            ReferralStatus.Draft,
            referral.Status);

        Assert.Equal(
            historyCountBefore,
            referral.History.Count);
    }

    [Fact]
    public void Complete_AddsCompletedHistoryEntry()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-009",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();
        referral.StartProgress();

        // Act
        referral.Complete();

        // Assert
        var historyEntry =
            Assert.Single(
                referral.History,
                entry =>
                    entry.EventType ==
                    ReferralHistoryEventType.Completed);

        Assert.Equal(
            ReferralStatus.InProgress,
            historyEntry.FromStatus);

        Assert.Equal(
            ReferralStatus.Completed,
            historyEntry.ToStatus);

        Assert.Equal(
            referral.UpdatedAt,
            historyEntry.OccurredAt);
    }

    // =========================================================
    // 4G - Appointment eligibility
    // =========================================================

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsDraft_ReturnsFalse()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-010",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsAccepted_ReturnsFalse()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-011",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsAssigned_ReturnsTrue()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-012",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsScheduled_ReturnsTrue()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-013",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsInProgress_ReturnsTrue()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-014",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();
        referral.StartProgress();

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsCompleted_ReturnsFalse()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-015",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Accept();
        referral.Assign(
            "Cardiology Team");

        referral.Schedule();
        referral.StartProgress();
        referral.Complete();

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void CanScheduleAppointment_WhenReferralIsRejected_ReturnsFalse()
    {
        // Arrange
        var referral =
            new Referral(
                "REF-4G-016",
                Guid.NewGuid(),
                ReferralPriority.Routine,
                "Test referral");

        referral.Submit();
        referral.StartTriage();
        referral.Reject();

        // Act
        var result =
            referral.CanScheduleAppointment();

        // Assert
        Assert.False(result);
    }
}