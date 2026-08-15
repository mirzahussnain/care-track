namespace CareTrack.Application.Common.Exceptions;

public sealed class InvalidStateTransitionException
    : Exception
{
  public InvalidStateTransitionException(
      string message)
      : base(message)
  {
  }
}