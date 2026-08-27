using CareTrack.DemoSeeder;
using CareTrack.Domain.Enums;

namespace CareTrack.UnitTests.DemoSeeder;

public sealed class DemoDatasetFactoryTests
{
  private static readonly DateTime Anchor =
      new(2026, 8, 27, 18, 30, 0, DateTimeKind.Utc);

  [Fact]
  public void Create_ProducesTheCuratedBaselineCounts()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);

    Assert.Equal(12, dataset.Patients.Count);
    Assert.Equal(17, dataset.Referrals.Count);
    Assert.Equal(94, dataset.ReferralHistoryCount);
    Assert.Equal(10, dataset.Appointments.Count);
    Assert.Equal(7, dataset.ClinicalNotes.Count);
  }

  [Fact]
  public void Create_ProducesTheExpectedReferralDistribution()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);
    var distribution = dataset.Referrals
        .GroupBy(referral => referral.Status)
        .ToDictionary(group => group.Key, group => group.Count());

    Assert.Equal(1, distribution[ReferralStatus.Draft]);
    Assert.Equal(1, distribution[ReferralStatus.Submitted]);
    Assert.Equal(2, distribution[ReferralStatus.AwaitingTriage]);
    Assert.Equal(2, distribution[ReferralStatus.MoreInformationRequired]);
    Assert.Equal(2, distribution[ReferralStatus.Accepted]);
    Assert.Equal(2, distribution[ReferralStatus.Assigned]);
    Assert.Equal(3, distribution[ReferralStatus.Scheduled]);
    Assert.Equal(2, distribution[ReferralStatus.InProgress]);
    Assert.Equal(1, distribution[ReferralStatus.Completed]);
    Assert.Equal(1, distribution[ReferralStatus.Rejected]);
    Assert.False(distribution.ContainsKey(ReferralStatus.Cancelled));
    Assert.Equal(
        9,
        dataset.Referrals.Count(referral =>
            referral.Priority == ReferralPriority.Routine));
    Assert.Equal(
        8,
        dataset.Referrals.Count(referral =>
            referral.Priority == ReferralPriority.Urgent));
  }

  [Fact]
  public void Create_ProducesTheExpectedAppointmentDistribution()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);
    var distribution = dataset.Appointments
        .GroupBy(appointment => appointment.Status)
        .ToDictionary(group => group.Key, group => group.Count());

    Assert.Equal(2, distribution[AppointmentStatus.Scheduled]);
    Assert.Equal(1, distribution[AppointmentStatus.CheckedIn]);
    Assert.Equal(1, distribution[AppointmentStatus.InProgress]);
    Assert.Equal(3, distribution[AppointmentStatus.Completed]);
    Assert.Equal(2, distribution[AppointmentStatus.Cancelled]);
    Assert.Equal(1, distribution[AppointmentStatus.DidNotAttend]);
  }

  [Fact]
  public void Create_UsesStableIdsAndTimestampsForTheSameAnchorDay()
  {
    var first = DemoDatasetFactory.Create(
        Anchor);
    var second = DemoDatasetFactory.Create(
        Anchor.AddHours(4));

    Assert.Equal(
        first.Patients.Select(patient =>
            (patient.Id, patient.PatientReference, patient.CreatedAt)),
        second.Patients.Select(patient =>
            (patient.Id, patient.PatientReference, patient.CreatedAt)));
    Assert.Equal(
        first.Referrals.Select(referral =>
            (referral.Id, referral.ReferralReference, referral.Status, referral.CreatedAt)),
        second.Referrals.Select(referral =>
            (referral.Id, referral.ReferralReference, referral.Status, referral.CreatedAt)));
    Assert.Equal(
        first.Appointments.Select(appointment =>
            (appointment.Id, appointment.AppointmentReference, appointment.Status, appointment.ScheduledStart)),
        second.Appointments.Select(appointment =>
            (appointment.Id, appointment.AppointmentReference, appointment.Status, appointment.ScheduledStart)));
    Assert.Equal(
        first.ClinicalNotes.Select(note =>
            (note.Id, note.AppointmentId, note.CreatedAt)),
        second.ClinicalNotes.Select(note =>
            (note.Id, note.AppointmentId, note.CreatedAt)));
  }

  [Fact]
  public void Create_MaintainsForeignKeyRelationshipsAndClinicalAuthorship()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);
    var patientIds = dataset.Patients
        .Select(patient => patient.Id)
        .ToHashSet();
    var referrals = dataset.Referrals
        .ToDictionary(referral => referral.Id);
    var appointmentIds = dataset.Appointments
        .Select(appointment => appointment.Id)
        .ToHashSet();

    Assert.All(
        dataset.Referrals,
        referral =>
        {
          Assert.Contains(referral.PatientId, patientIds);
          Assert.All(
              referral.History,
              history => Assert.Equal(referral.Id, history.ReferralId));
        });
    Assert.All(
        dataset.Appointments,
        appointment =>
        {
          Assert.Contains(appointment.PatientId, patientIds);
          Assert.True(
              referrals.TryGetValue(
                  appointment.ReferralId,
                  out var referral));
          Assert.NotNull(referral);
          Assert.Equal(referral.PatientId, appointment.PatientId);
        });
    Assert.All(
        dataset.ClinicalNotes,
        note =>
        {
          Assert.Contains(note.AppointmentId, appointmentIds);
          Assert.Equal(
              DemoDatasetFactory.ClinicianObjectId,
              note.CreatedBy);
        });
  }

  [Fact]
  public void Create_HasNoActiveAppointmentOverlapForAnyPatient()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);
    var activeAppointments = dataset.Appointments
        .Where(appointment => appointment.Status is not AppointmentStatus.Cancelled
            and not AppointmentStatus.DidNotAttend)
        .GroupBy(appointment => appointment.PatientId);

    foreach (var patientAppointments in activeAppointments)
    {
      var ordered = patientAppointments
          .OrderBy(appointment => appointment.ScheduledStart)
          .ToArray();

      for (var index = 1; index < ordered.Length; index++)
      {
        Assert.True(
            ordered[index - 1].ScheduledEnd <= ordered[index].ScheduledStart,
            $"Appointments overlap for demo patient '{patientAppointments.Key}'.");
      }
    }
  }

  [Fact]
  public void Create_UsesOnlyClearlyDemoReferencesAndSyntheticNarrative()
  {
    var dataset = DemoDatasetFactory.Create(
        Anchor);

    Assert.All(
        dataset.Patients,
        patient => Assert.StartsWith(
            "DEMO-PAT-",
            patient.PatientReference,
            StringComparison.Ordinal));
    Assert.All(
        dataset.Referrals,
        referral =>
        {
          Assert.StartsWith(
              "DEMO-REF-",
              referral.ReferralReference,
              StringComparison.Ordinal);
          Assert.Contains(
              "Synthetic demo",
              referral.Reason,
              StringComparison.Ordinal);
        });
    Assert.All(
        dataset.Appointments,
        appointment => Assert.StartsWith(
            "DEMO-APT-",
            appointment.AppointmentReference,
            StringComparison.Ordinal));
  }

  [Fact]
  public void Create_RejectsANonUtcAnchor()
  {
    var localAnchor = DateTime.SpecifyKind(
        Anchor,
        DateTimeKind.Local);

    Assert.Throws<ArgumentException>(() =>
        DemoDatasetFactory.Create(localAnchor));
  }
}
