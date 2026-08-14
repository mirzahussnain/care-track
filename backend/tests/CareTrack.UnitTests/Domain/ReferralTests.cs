using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Helpers;
namespace CareTrack.UnitTests.Domain;


public class ReferralTests
{


  [Fact]
  public void Constructor_WithValidValues_CreatesDraftReferral()
  {
    var patientId = Guid.NewGuid();
    var referral = new Referral("REF-001", patientId, ReferralPriority.Routine, "Persistent shoulder pain.");
    Assert.NotEqual(Guid.Empty, referral.Id);
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

  //Trimming Test
  [Fact]
  public void Constructor_TrimsReferenceAndReason()
  {
    var referral =
        new Referral(
            "  REF-001  ",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "  Shoulder pain  ");

    Assert.Equal(
        "REF-001",
        referral.ReferralReference);

    Assert.Equal(
        "Shoulder pain",
        referral.Reason);
  }
  //Test Empty Reference
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
  //Test Empty Patient Id
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
  //Test Blank Reason
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

  //Test Submit
  [Fact]
  public void Submit_WhenDraft_ChangesStatusToSubmitted()
  {
    var referral =
        new Referral(
            "REF-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();

    Assert.Equal(
        ReferralStatus.Submitted,
        referral.Status);

    Assert.NotNull(
        referral.SubmittedAt);

    Assert.NotNull(
        referral.UpdatedAt);
  }
  //Test Duplicate Submit
  [Fact]
  public void Submit_WhenAlreadySubmitted_ThrowsInvalidOperationException()
  {
    var referral =
        new Referral(
            "REF-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();

    Assert.Throws<InvalidOperationException>(
        () => referral.Submit());
  }

  [Fact]
  public void StartTriage_WhenSubmitted_ChangesStatusToAwaitingTriage()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-TRIAGE-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

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
    var referral =
        new Referral(
            "REF-TRIAGE-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    Assert.Throws<InvalidOperationException>(
        () => referral.StartTriage());

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);
  }

  [Fact]
  public void Accept_WhenAwaitingTriage_ChangesStatusToAccepted()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.Accept();

    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);

    Assert.NotNull(
        referral.UpdatedAt);
  }

  [Fact]
  public void Accept_WhenSubmitted_ThrowsInvalidOperationException()
  {
    var referral =
        new Referral(
            "REF-ACC-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();

    Assert.Throws<InvalidOperationException>(
        () => referral.Accept());

    Assert.Equal(
        ReferralStatus.Submitted,
        referral.Status);
  }

  [Fact]
  public void Reject_WhenAwaitingTriage_ChangesStatusToRejected()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.Reject();

    Assert.Equal(
        ReferralStatus.Rejected,
        referral.Status);
  }

  [Fact]
  public void RequestMoreInformation_WhenAwaitingTriage_ChangesStatus()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.RequestMoreInformation();

    Assert.Equal(
        ReferralStatus.MoreInformationRequired,
        referral.Status);
  }

  [Fact]
  public void Resubmit_WhenMoreInformationRequired_ReturnsToSubmitted()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.RequestMoreInformation();

    var originalSubmittedAt =
        referral.SubmittedAt;

    referral.Resubmit();

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
    var referral =
        new Referral(
            "REF-RESUB-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    Assert.Throws<InvalidOperationException>(
        () => referral.Resubmit());

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);
  }

  [Fact]
  public void StartTriage_WhenAccepted_ThrowsInvalidOperationException()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.Accept();

    Assert.Throws<InvalidOperationException>(
        () => referral.StartTriage());

    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);
  }

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
    var referral =
        new Referral(
            "REF-TRIAGE-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

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
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

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
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    var longNote =
        new string('A', 2001);

    Assert.Throws<ArgumentException>(
        () => referral.RecordTriageAssessment(
            ReferralPriority.Urgent,
            longNote));
  }

  [Fact]
  public void RecordTriageAssessment_WithInvalidPriority_ThrowsArgumentOutOfRangeException()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    var invalidPriority =
        (ReferralPriority)999;

    Assert.Throws<ArgumentOutOfRangeException>(
        () => referral.RecordTriageAssessment(
            invalidPriority,
            "Triage note."));
  }

  [Fact]
  public void RecordTriageAssessment_TrimsTriageNote()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.RecordTriageAssessment(
        ReferralPriority.Routine,
        "  Suitable for routine review.  ");

    Assert.Equal(
        "Suitable for routine review.",
        referral.TriageNote);
  }

  [Fact]
  public void RecordTriageAssessment_WhenCalledAgain_ReplacesCurrentTriageAssessment()
  {
    var referral =
        ReferralTestHelpers.CreateAwaitingTriageReferral();

    referral.RecordTriageAssessment(
        ReferralPriority.Routine,
        "Initially suitable for routine review.");

    var firstTriagedAt =
        referral.TriagedAt;

    referral.RecordTriageAssessment(
        ReferralPriority.Urgent,
        "Condition deteriorated.");

    Assert.Equal(
        ReferralPriority.Urgent,
        referral.Priority);

    Assert.Equal(
        "Condition deteriorated.",
        referral.TriageNote);

    Assert.True(
        referral.TriagedAt >= firstTriagedAt);
  }

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
    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    referral.Assign(
        "  Cardiology Team A  ");

    Assert.Equal(
        "Cardiology Team A",
        referral.AssignedTo);
  }

  [Fact]
  public void Assign_WhenDraft_ThrowsInvalidOperationException()
  {
    var referral =
        new Referral(
            "REF-ASSIGN-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

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
    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    Assert.Throws<ArgumentException>(
        () => referral.Assign("   "));

    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);

    Assert.Null(
        referral.AssignedTo);
  }

  [Fact]
  public void Assign_WithTargetLongerThan200Characters_ThrowsArgumentException()
  {
    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    var target =
        new string('A', 201);

    Assert.Throws<ArgumentException>(
        () => referral.Assign(target));

    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);
  }

  [Fact]
  public void Reassign_WhenAssigned_UpdatesAssignment()
  {
    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    referral.Assign(
        "Cardiology Team A");

    var firstAssignedAt =
        referral.AssignedAt;

    referral.Reassign(
        "Cardiology Team B");

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
    var referral =
        ReferralTestHelpers.CreateAcceptedReferral();

    Assert.Throws<InvalidOperationException>(
        () => referral.Reassign(
            "Cardiology Team B"));

    Assert.Equal(
        ReferralStatus.Accepted,
        referral.Status);

    Assert.Null(
        referral.AssignedTo);
  }
}