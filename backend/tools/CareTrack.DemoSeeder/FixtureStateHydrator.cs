using System.Reflection;

namespace CareTrack.DemoSeeder;

internal static class FixtureStateHydrator
{
  public static void Set<T>(
      object target,
      string propertyName,
      T value)
  {
    var property = target.GetType().GetProperty(
        propertyName,
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException(
            $"Fixture property '{propertyName}' was not found on '{target.GetType().Name}'.");

    property.SetValue(target, value);
  }
}
