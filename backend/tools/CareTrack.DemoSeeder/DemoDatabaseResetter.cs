using System.Data;
using CareTrack.Infrastructure.Persistance;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.DemoSeeder;

public sealed class DemoDatabaseResetter(
    CareTrackDbContext dbContext)
{
  public async Task<DemoSeedCounts> ResetAsync(
      DemoSeedDataset dataset,
      CancellationToken cancellationToken = default)
  {
    var executionStrategy = dbContext.Database
        .CreateExecutionStrategy();

    return await executionStrategy.ExecuteAsync(
        () => ResetOnceAsync(
            dataset,
            cancellationToken));
  }

  private async Task<DemoSeedCounts> ResetOnceAsync(
      DemoSeedDataset dataset,
      CancellationToken cancellationToken)
  {
    dbContext.ChangeTracker.Clear();

    var migrationsBefore = (await dbContext.Database
            .GetAppliedMigrationsAsync(cancellationToken))
        .ToArray();

    await using var transaction = await dbContext.Database
        .BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

    await dbContext.ClinicalNotes.ExecuteDeleteAsync(
        cancellationToken);
    await dbContext.Appointments.ExecuteDeleteAsync(
        cancellationToken);
    await dbContext.Referrals.ExecuteDeleteAsync(
        cancellationToken);
    await dbContext.Patients.ExecuteDeleteAsync(
        cancellationToken);

    dbContext.ChangeTracker.Clear();

    dbContext.Patients.AddRange(dataset.Patients);
    dbContext.Referrals.AddRange(dataset.Referrals);
    dbContext.Appointments.AddRange(dataset.Appointments);
    dbContext.ClinicalNotes.AddRange(dataset.ClinicalNotes);

    await dbContext.SaveChangesAsync(
        cancellationToken);

    var counts = new DemoSeedCounts(
        await dbContext.Patients.CountAsync(cancellationToken),
        await dbContext.Referrals.CountAsync(cancellationToken),
        await dbContext.ReferralHistoryEntries.CountAsync(cancellationToken),
        await dbContext.Appointments.CountAsync(cancellationToken),
        await dbContext.ClinicalNotes.CountAsync(cancellationToken));

    var migrationsAfter = (await dbContext.Database
            .GetAppliedMigrationsAsync(cancellationToken))
        .ToArray();

    if (!migrationsBefore.SequenceEqual(
            migrationsAfter,
            StringComparer.Ordinal))
    {
      throw new InvalidOperationException(
          "The EF migration history changed during the demo reset.");
    }

    await transaction.CommitAsync(
        cancellationToken);

    return counts;
  }
}
