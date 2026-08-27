namespace CareTrack.Api.Identity;

public interface IDemoAccountDirectory
{
  bool Contains(string objectId);
}

public sealed class DemoAccountDirectory : IDemoAccountDirectory
{
  private static readonly HashSet<string> DemoObjectIds = new(
      StringComparer.OrdinalIgnoreCase)
  {
    "3ab16ad9-5920-4082-b96d-4a967439240a",
    "20b23d69-e106-4b0e-96ff-8a60018232a1"
  };

  public bool Contains(string objectId)
  {
    return !string.IsNullOrWhiteSpace(objectId)
        && DemoObjectIds.Contains(objectId);
  }
}
