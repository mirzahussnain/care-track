using System.Data;
using CareTrack.Application.Common.Interfaces;
namespace CareTrack.UnitTests.TestSupport.Fakes;

public sealed class FakeApplicationTransaction
    : IApplicationTransaction
{
    public IsolationLevel? RequestedIsolationLevel { get; private set; }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await operation(
            cancellationToken);
    }

    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        RequestedIsolationLevel = isolationLevel;

        await operation(
            cancellationToken);
    }
}
