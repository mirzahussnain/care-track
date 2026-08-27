using CareTrack.DemoSeeder;
using CareTrack.Domain.Enums;

namespace CareTrack.UnitTests.DemoSeeder;

public sealed class DemoWorkflowScenarioTests
{
  private static readonly DemoSeedDataset Dataset =
      DemoDatasetFactory.Create(
          new DateTime(
              2026,
              8,
              27,
              18,
              30,
              0,
              DateTimeKind.Utc));

  [Fact]
  public void ActiveClinicalScenario_HasAnInProgressAppointmentAndReferral()
  {
    var referral = GetReferral("DEMO-REF-010");
    var appointment = Assert.Single(
        Dataset.Appointments,
        candidate => candidate.ReferralId == referral.Id);

    Assert.Equal(ReferralStatus.InProgress, referral.Status);
    Assert.Equal(AppointmentStatus.InProgress, appointment.Status);
    Assert.Contains(
        Dataset.ClinicalNotes,
        note => note.AppointmentId == appointment.Id);
  }

  [Fact]
  public void ReferralCompletionScenario_HasCompletedWorkAndNoActiveAppointment()
  {
    var referral = GetReferral("DEMO-REF-011");
    var appointments = Dataset.Appointments.Where(candidate =>
        candidate.ReferralId == referral.Id).ToArray();

    Assert.Equal(ReferralStatus.InProgress, referral.Status);
    Assert.Contains(
        appointments,
        appointment => appointment.Status == AppointmentStatus.Completed);
    Assert.DoesNotContain(
        appointments,
        appointment => appointment.Status is AppointmentStatus.Scheduled
            or AppointmentStatus.CheckedIn
            or AppointmentStatus.InProgress);
  }

  [Fact]
  public void HistoricalScenario_HasACompleteReferralJourneyAndTwoCompletedAppointments()
  {
    var referral = GetReferral("DEMO-REF-012");
    var appointments = Dataset.Appointments.Where(candidate =>
        candidate.ReferralId == referral.Id).ToArray();

    Assert.Equal(ReferralStatus.Completed, referral.Status);
    Assert.Equal(2, appointments.Length);
    Assert.All(
        appointments,
        appointment => Assert.Equal(
            AppointmentStatus.Completed,
            appointment.Status));
    Assert.Contains(
        referral.History,
        entry => entry.EventType == ReferralHistoryEventType.Completed);
  }

  [Fact]
  public void EveryReferralHistoryStartsWithCreationAndEndsAtItsCurrentState()
  {
    Assert.All(
        Dataset.Referrals,
        referral =>
        {
          var ordered = referral.History
              .OrderBy(entry => entry.OccurredAt)
              .ToArray();

          Assert.Equal(
              ReferralHistoryEventType.Created,
              ordered[0].EventType);
          Assert.Equal(
              ReferralStatus.Draft,
              ordered[0].ToStatus);
          Assert.Equal(
              referral.Status,
              ordered[^1].ToStatus);
        });
  }

  private static CareTrack.Domain.Entities.Referral GetReferral(
      string reference)
  {
    return Assert.Single(
        Dataset.Referrals,
        referral => referral.ReferralReference == reference);
  }
}
