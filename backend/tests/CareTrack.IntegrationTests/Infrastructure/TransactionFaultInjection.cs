using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace CareTrack.IntegrationTests.Infrastructure;

internal enum CommitFaultMode
{
  TransientBeforeCommit,
  TransientAfterCommit,
  NonTransientBeforeCommit
}

internal sealed class CommitFaultState
{
  private int _faultRemaining = 1;
  private int _commitAttempts;

  public CommitFaultState(
      CommitFaultMode mode)
  {
    Mode = mode;
  }

  public CommitFaultMode Mode { get; }

  public int CommitAttempts =>
      Volatile.Read(ref _commitAttempts);

  public void RecordCommitAttempt()
  {
    Interlocked.Increment(ref _commitAttempts);
  }

  public bool TakeFault()
  {
    return Interlocked.Exchange(
        ref _faultRemaining,
        0) == 1;
  }
}

internal sealed class CommitFaultInterceptor
    : DbTransactionInterceptor
{
  private readonly CommitFaultState _state;

  public CommitFaultInterceptor(
      CommitFaultState state)
  {
    _state = state;
  }

  public override ValueTask<InterceptionResult>
      TransactionCommittingAsync(
          DbTransaction transaction,
          TransactionEventData eventData,
          InterceptionResult result,
          CancellationToken cancellationToken = default)
  {
    _state.RecordCommitAttempt();

    if (_state.Mode == CommitFaultMode.TransientBeforeCommit
        && _state.TakeFault())
    {
      throw new TestTransientException();
    }

    if (_state.Mode == CommitFaultMode.NonTransientBeforeCommit
        && _state.TakeFault())
    {
      throw new TestNonTransientException();
    }

    return base.TransactionCommittingAsync(
        transaction,
        eventData,
        result,
        cancellationToken);
  }

  public override Task TransactionCommittedAsync(
      DbTransaction transaction,
      TransactionEndEventData eventData,
      CancellationToken cancellationToken = default)
  {
    if (_state.Mode == CommitFaultMode.TransientAfterCommit
        && _state.TakeFault())
    {
      throw new TestTransientException();
    }

    return base.TransactionCommittedAsync(
        transaction,
        eventData,
        cancellationToken);
  }
}

internal sealed class TestExecutionStrategy
    : ExecutionStrategy
{
  public TestExecutionStrategy(
      ExecutionStrategyDependencies dependencies)
      : base(
          dependencies,
          maxRetryCount: 2,
          maxRetryDelay: TimeSpan.Zero)
  {
  }

  protected override bool ShouldRetryOn(
      Exception exception)
  {
    return exception is TestTransientException;
  }

  protected override bool ShouldVerifySuccessOn(
      Exception exception)
  {
    return exception is TestTransientException;
  }
}

internal sealed class TestTransientException : Exception;

internal sealed class TestNonTransientException : Exception;
