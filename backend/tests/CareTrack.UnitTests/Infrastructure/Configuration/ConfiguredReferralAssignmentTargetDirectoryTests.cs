using CareTrack.Infrastructure.Configuration;

namespace CareTrack.UnitTests.Infrastructure.Configuration;

public sealed class ConfiguredReferralAssignmentTargetDirectoryTests
{
  [Fact]
  public void Constructor_WithNoTargets_Throws()
  {
    Assert.Throws<InvalidOperationException>(
        () => new ConfiguredReferralAssignmentTargetDirectory([]));
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void Constructor_WithBlankTarget_Throws(
      string target)
  {
    Assert.Throws<InvalidOperationException>(
        () => new ConfiguredReferralAssignmentTargetDirectory([target]));
  }

  [Fact]
  public void Constructor_WithOverlongTarget_Throws()
  {
    Assert.Throws<InvalidOperationException>(
        () => new ConfiguredReferralAssignmentTargetDirectory(
            [new string('T', 201)]));
  }

  [Fact]
  public void Constructor_WithCaseInsensitiveDuplicate_Throws()
  {
    Assert.Throws<InvalidOperationException>(
        () => new ConfiguredReferralAssignmentTargetDirectory(
            ["Cardiology Team A", "cardiology team a"]));
  }

  [Fact]
  public void Constructor_WithValidTargets_PreservesTrimmedConfiguredValues()
  {
    var directory = new ConfiguredReferralAssignmentTargetDirectory(
        [" Cardiology Team A ", "Respiratory Team"]);

    Assert.Equal(
        ["Cardiology Team A", "Respiratory Team"],
        directory.Targets);
  }

  [Fact]
  public void ResolveCanonicalName_WithDifferentCase_ReturnsConfiguredName()
  {
    var directory = new ConfiguredReferralAssignmentTargetDirectory(
        ["Cardiology Team A"]);

    var result = directory.ResolveCanonicalName("  cardiology team a  ");

    Assert.Equal("Cardiology Team A", result);
  }

  [Theory]
  [InlineData("")]
  [InlineData("   ")]
  public void ResolveCanonicalName_WithBlankRequestedTarget_Throws(
      string target)
  {
    var directory = new ConfiguredReferralAssignmentTargetDirectory(
        ["Cardiology Team A"]);

    Assert.Throws<ArgumentException>(
        () => directory.ResolveCanonicalName(target));
  }

  [Fact]
  public void ResolveCanonicalName_WithUnavailableTarget_Throws()
  {
    var directory = new ConfiguredReferralAssignmentTargetDirectory(
        ["Cardiology Team A"]);

    Assert.Throws<ArgumentException>(
        () => directory.ResolveCanonicalName("Unknown Team"));
  }
}
