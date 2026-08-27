using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace CareTrack.Api.Observability;

public readonly record struct DatabaseFailureMetadata(
    string Category,
    int? SqlErrorNumber,
    bool RetryExhausted)
{
  public static DatabaseFailureMetadata From(
      Exception exception)
  {
    SqlException? sqlException = null;

    for (Exception? current = exception;
         current is not null;
         current = current.InnerException)
    {
      if (current is SqlException candidate)
      {
        sqlException = candidate;
        break;
      }
    }

    var retryExhausted =
        exception is RetryLimitExceededException;

    if (sqlException is not null)
    {
      return new DatabaseFailureMetadata(
          "SqlException",
          sqlException.Number,
          retryExhausted);
    }

    return new DatabaseFailureMetadata(
        exception.GetType().Name,
        null,
        retryExhausted);
  }
}
