using CareTrack.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _dbContext.Database
                .BeginTransactionAsync(
                    cancellationToken);

        try
        {
            await operation(
                cancellationToken);

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}