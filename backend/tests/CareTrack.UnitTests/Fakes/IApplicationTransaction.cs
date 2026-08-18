using CareTrack.Application.Common.Interfaces;
namespace CareTrack.UnitTests.Fakes;

public sealed class FakeApplicationTransaction
    : IApplicationTransaction
{
    public async Task ExecuteAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        await operation(
            cancellationToken);
    }
}