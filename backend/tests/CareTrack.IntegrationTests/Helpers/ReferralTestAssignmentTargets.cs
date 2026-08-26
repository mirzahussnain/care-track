namespace CareTrack.IntegrationTests.Helpers;

public static class ReferralTestAssignmentTargets
{
  public const string CardiologyTeamA = "Cardiology Team A";
  public const string CardiologyTeamB = "Cardiology Team B";
  public const string RespiratoryTeam = "Respiratory Team";
  public const string Default = "Integration Test Team";

  public static IReadOnlyList<string> All { get; } =
  [
    CardiologyTeamA,
    CardiologyTeamB,
    RespiratoryTeam,
    Default
  ];
}
