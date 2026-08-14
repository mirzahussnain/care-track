using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

namespace CareTrack.UnitTests.Helpers;

public class ReferralTestHelpers
{
  public static Referral CreateAcceptedReferral()
  {
    var referral =
        new Referral(
            $"REF-{Guid.NewGuid():N}"[..12],
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Referral reason.");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();

    return referral;
  }

  public static Referral CreateAwaitingTriageReferral()
  {
    var referral =
        new Referral(
            $"REF-{Guid.NewGuid():N}"[..12],
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Reason");

    referral.Submit();
    referral.StartTriage();

    return referral;
  }
}