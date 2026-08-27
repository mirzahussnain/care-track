using System.Data;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CareTrack.Infrastructure.Persistance;

public sealed class ApplicationTransaction
    : IApplicationTransaction
{
    private readonly CareTrackDbContext _dbContext;

    public ApplicationTransaction(
        CareTrackDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<CancellationToken, Task<bool>> verifySucceeded,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            operation,
            verifySucceeded,
            IsolationLevel.ReadCommitted,
            cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        Func<CancellationToken, Task<bool>> verifySucceeded,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var executionStrategy =
                _dbContext.Database
                    .CreateExecutionStrategy();

            await executionStrategy.ExecuteInTransactionAsync(
                async ct =>
                {
                    // Start every attempt without entities retained by a
                    // previous failed or commit-ambiguous attempt.
                    _dbContext.ChangeTracker.Clear();

                    await operation(ct);
                },
                async ct =>
                {
                    // Verification must query persisted state, not the
                    // entities retained by the uncertain attempt.
                    _dbContext.ChangeTracker.Clear();

                    return await verifySucceeded(ct);
                },
                isolationLevel,
                cancellationToken);
        }
        catch (Exception exception)
        {
            if (IsSqlServerDeadlockVictim(exception))
            {
                throw new ConcurrencyException(
                    "The operation could not be completed because of a concurrent change. Please retry.",
                    exception);
            }

            throw;
        }
    }

    private static bool IsSqlServerDeadlockVictim(
        Exception exception)
    {
        for (Exception? current = exception;
             current is not null;
             current = current.InnerException)
        {
            if (current is SqlException { Number: 1205 })
            {
                return true;
            }
        }

        return false;
    }
}
