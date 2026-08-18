using System.Data;

namespace CareTrack.Application.Common.Interfaces;

public interface IApplicationTransaction
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);

    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default);
}
