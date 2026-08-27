using CareTrack.DemoSeeder;

namespace CareTrack.UnitTests.DemoSeeder;

public sealed class DemoSeederApplicationTests
{
  [Fact]
  public void HasRequiredTargetArgument_RejectsAnythingExceptTheExactTarget()
  {
    string[][] invalidArguments =
    [
      [],
      ["--target-database"],
      ["--target-database", "CareTrackIntegrationTests"],
      ["--target-database", "caretrackdb"],
      ["--target-database", "CareTrackDb", "--yes"]
    ];

    Assert.All(
        invalidArguments,
        args => Assert.False(
            DemoSeederApplication.HasRequiredTargetArgument(args)));
  }

  [Fact]
  public void HasRequiredTargetArgument_AcceptsOnlyCareTrackDb()
  {
    Assert.True(
        DemoSeederApplication.HasRequiredTargetArgument(
            ["--target-database", "CareTrackDb"]));
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("reset CareTrackDb")]
  [InlineData("RESET caretrackdb")]
  [InlineData("RESET CareTrackDb ")]
  public void IsExactConfirmation_RejectsNonExactValues(
      string? value)
  {
    Assert.False(
        DemoSeederApplication.IsExactConfirmation(value));
  }

  [Fact]
  public void IsExactConfirmation_AcceptsTheExactPhrase()
  {
    Assert.True(
        DemoSeederApplication.IsExactConfirmation(
            "RESET CareTrackDb"));
  }

  [Fact]
  public async Task RunAsync_RefusesToRunWithoutAConnectionString()
  {
    using var output = new StringWriter();
    using var error = new StringWriter();

    var exitCode = await DemoSeederApplication.RunAsync(
        ["--target-database", "CareTrackDb"],
        () => null,
        new StringReader("RESET CareTrackDb"),
        output,
        error);

    Assert.Equal(2, exitCode);
    Assert.Empty(output.ToString());
    Assert.Contains(
        DemoSeederApplication.ConnectionStringEnvironmentVariable,
        error.ToString(),
        StringComparison.Ordinal);
    Assert.Contains(
        "No records were changed",
        error.ToString(),
        StringComparison.Ordinal);
  }

  [Fact]
  public async Task RunAsync_NeverWritesTheSuppliedConnectionStringOnFailure()
  {
    const string sentinel =
        "Server=SECRET-SERVER;Database=CareTrackDb;User Id=SECRET-USER;Password=SECRET-PASSWORD;Invalid Keyword=value";
    using var output = new StringWriter();
    using var error = new StringWriter();

    var exitCode = await DemoSeederApplication.RunAsync(
        ["--target-database", "CareTrackDb"],
        () => sentinel,
        new StringReader("RESET CareTrackDb"),
        output,
        error);

    Assert.Equal(5, exitCode);
    var combinedOutput = output + error.ToString();
    Assert.DoesNotContain(
        sentinel,
        combinedOutput,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "SECRET-SERVER",
        combinedOutput,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "SECRET-USER",
        combinedOutput,
        StringComparison.Ordinal);
    Assert.DoesNotContain(
        "SECRET-PASSWORD",
        combinedOutput,
        StringComparison.Ordinal);
  }
}
