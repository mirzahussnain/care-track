using System.Collections.Concurrent;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CareTrack.IntegrationTests.Infrastructure;

public sealed class SqlCommandRecorder : DbCommandInterceptor
{
  private readonly ConcurrentQueue<string> _commandTexts = new();

  public IReadOnlyList<string> CommandTexts => _commandTexts.ToArray();

  public void Clear()
  {
    while (_commandTexts.TryDequeue(out _))
    {
    }
  }

  public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
      DbCommand command,
      CommandEventData eventData,
      InterceptionResult<DbDataReader> result,
      CancellationToken cancellationToken = default)
  {
    _commandTexts.Enqueue(command.CommandText);
    return base.ReaderExecutingAsync(
        command, eventData, result, cancellationToken);
  }
}
