namespace CareTrack.Application.Common.Exceptions;

public class ConflictException : Exception
{
  public ConflictException(string message)
      : base(message)
  {
  }
}

public class NotFoundException : Exception
{
  public NotFoundException(string message)
      : base(message)
  {
  }
}