using System.Data;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Common.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CareTrack.Infrastructure.Persistance;

public sealed class ApplicationTransaction
    : IApplicationTransaction
{
    private readonly CareTrackDbContext _dbContext;
    private readonly ILogger<ApplicationTransaction> _logger;

    public ApplicationTransaction(
        CareTrackDbContext dbContext,
        ILogger<ApplicationTransaction> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            operation,
            IsolationLevel.ReadCommitted,
            cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    isolationLevel,
                    cancellationToken);

        try
        {
            await operation(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch (Exception exception)
        {
            try
            {
                await transaction.RollbackAsync(
                    CancellationToken.None);
            }
            catch (Exception rollbackException)
            {
                _logger.LogError(
                    rollbackException,
                    "Transaction rollback failed while handling an operation failure.");
            }

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
