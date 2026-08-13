using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
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
}