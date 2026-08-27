using System.Security.Cryptography;
using System.Text;

namespace CareTrack.DemoSeeder;

internal static class StableDemoGuid
{
  public static Guid For(string logicalKey)
  {
    var input = Encoding.UTF8.GetBytes(
        $"caretrack-demo-v1:{logicalKey}");
    var hash = SHA256.HashData(input);
    var bytes = hash[..16];

    bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50);
    bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);

    return new Guid(bytes);
  }
}
