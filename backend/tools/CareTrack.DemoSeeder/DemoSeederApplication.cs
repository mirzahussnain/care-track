using System.Data;
using CareTrack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.DemoSeeder;

public static class DemoSeederApplication
{
  public const string ConnectionStringEnvironmentVariable =
      "CARETRACK_DEMO_DB_CONNECTION_STRING";
  public const string RequiredTargetDatabase =
      "CareTrackDb";
  public const string RequiredConfirmation =
      "RESET CareTrackDb";

  public static async Task<int> RunAsync(
      string[] args,
      Func<string?> connectionStringProvider,
      TextReader input,
      TextWriter output,
      TextWriter error,
      CancellationToken cancellationToken = default)
  {
    if (!HasRequiredTargetArgument(args))
    {
      await error.WriteLineAsync(
          "Usage requires --target-database CareTrackDb.");
      return 2;
    }

    var connectionString = connectionStringProvider();

    if (string.IsNullOrWhiteSpace(connectionString))
    {
      await error.WriteLineAsync(
          $"{ConnectionStringEnvironmentVariable} is required. No records were changed.");
      return 2;
    }

    try
    {
      var options = new DbContextOptionsBuilder<CareTrackDbContext>()
          .UseSqlServer(
              connectionString,
              sqlServerOptions => sqlServerOptions.EnableRetryOnFailure(
                  maxRetryCount: 3,
                  maxRetryDelay: TimeSpan.FromSeconds(5),
                  errorNumbersToAdd: null))
          .EnableSensitiveDataLogging(false)
          .Options;

      await using var dbContext = new CareTrackDbContext(options);
      await dbContext.Database.OpenConnectionAsync(
          cancellationToken);

      var databaseName = await ResolveDatabaseNameAsync(
          dbContext,
          cancellationToken);

      if (!string.Equals(
              databaseName,
              RequiredTargetDatabase,
              StringComparison.Ordinal))
      {
        await error.WriteLineAsync(
            "Connected database does not match the requested target. No records were changed.");
        return 3;
      }

      var pendingMigrations = (await dbContext.Database
              .GetPendingMigrationsAsync(cancellationToken))
          .ToArray();

      if (pendingMigrations.Length > 0)
      {
        await error.WriteLineAsync(
            "The target database has pending migrations. No records were changed.");
        return 3;
      }

      await output.WriteLineAsync(
          "Target database: CareTrackDb");
      await output.WriteLineAsync(
          "Operation: destructive reset of CareTrack domain demo records");
      await output.WriteLineAsync(
          "Migration history: preserved");

      var confirmation = await input.ReadLineAsync(
          cancellationToken);

      if (!IsExactConfirmation(confirmation))
      {
        await error.WriteLineAsync(
            "Confirmation did not match. No records were changed.");
        return 4;
      }

      var dataset = DemoDatasetFactory.Create(
          DateTime.UtcNow);
      var resetter = new DemoDatabaseResetter(
          dbContext);
      var counts = await resetter.ResetAsync(
          dataset,
          cancellationToken);

      await output.WriteLineAsync(
          "Reset completed.");
      await output.WriteLineAsync(
          $"Patients: {counts.Patients}");
      await output.WriteLineAsync(
          $"Referrals: {counts.Referrals}");
      await output.WriteLineAsync(
          $"Referral history entries: {counts.ReferralHistoryEntries}");
      await output.WriteLineAsync(
          $"Appointments: {counts.Appointments}");
      await output.WriteLineAsync(
          $"Clinical notes: {counts.ClinicalNotes}");

      return 0;
    }
    catch
    {
      await error.WriteLineAsync(
          "Seeder failed. No reset was committed.");
      return 5;
    }
  }

  public static bool HasRequiredTargetArgument(
      IReadOnlyList<string> args)
  {
    return args.Count == 2
        && string.Equals(
            args[0],
            "--target-database",
            StringComparison.Ordinal)
        && string.Equals(
            args[1],
            RequiredTargetDatabase,
            StringComparison.Ordinal);
  }

  public static bool IsExactConfirmation(
      string? value)
  {
    return string.Equals(
        value,
        RequiredConfirmation,
        StringComparison.Ordinal);
  }

  private static async Task<string?> ResolveDatabaseNameAsync(
      CareTrackDbContext dbContext,
      CancellationToken cancellationToken)
  {
    await using var command = dbContext.Database
        .GetDbConnection()
        .CreateCommand();
    command.CommandText = "SELECT DB_NAME()";
    command.CommandType = CommandType.Text;

    var result = await command.ExecuteScalarAsync(
        cancellationToken);

    return result as string;
  }
}
