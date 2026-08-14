using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

namespace CareTrack.UnitTests.Helpers;

public class ReferralTestHelpers
{
  public static Referral CreateNewReferral()
  {
    var referral =
        new Referral(
            $"REF-{Guid.NewGuid():N}"[..12],
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Referral reason.");
    return referral;
  }
  public static Referral CreateAcceptedReferral()
  {
    var referral = CreateNewReferral();

    referral.Submit();
    referral.StartTriage();
    referral.Accept();

    return referral;
  }

  public static Referral CreateAwaitingTriageReferral()
  {
    var referral = CreateNewReferral();

    referral.Submit();
    referral.StartTriage();

    return referral;
  }
}