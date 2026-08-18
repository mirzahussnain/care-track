namespace CareTrack.Application.Common.Interfaces;

public interface IApplicationTransaction
{
    Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default);
}