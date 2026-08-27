using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

namespace CareTrack.DemoSeeder;

public static partial class DemoDatasetFactory
{
  public const string ClinicianObjectId =
      "20b23d69-e106-4b0e-96ff-8a60018232a1";

  public static DemoSeedDataset Create(
      DateTime utcAnchor)
  {
    if (utcAnchor.Kind != DateTimeKind.Utc)
    {
      throw new ArgumentException(
          "The demo dataset anchor must be UTC.",
          nameof(utcAnchor));
    }

    var anchor = utcAnchor.Date.AddHours(12);
    var patients = CreatePatients(anchor);
    var patientByReference = patients.ToDictionary(
        patient => patient.PatientReference,
        StringComparer.Ordinal);
    var referrals = CreateReferrals(
        anchor,
        patientByReference);
    var referralByReference = referrals.ToDictionary(
        referral => referral.ReferralReference,
        StringComparer.Ordinal);
    var appointments = CreateAppointments(
        anchor,
        referralByReference);
    var appointmentByReference = appointments.ToDictionary(
        appointment => appointment.AppointmentReference,
        StringComparer.Ordinal);
    var clinicalNotes = CreateClinicalNotes(
        anchor,
        appointmentByReference);

    return new DemoSeedDataset(
        patients,
        referrals,
        appointments,
        clinicalNotes);
  }

  private static IReadOnlyList<Patient> CreatePatients(
      DateTime anchor)
  {
    var definitions = new[]
    {
      new PatientDefinition("DEMO-PAT-001", "Amira", "Hartwell", new DateOnly(1991, 4, 12)),
      new PatientDefinition("DEMO-PAT-002", "Owen", "Mercer", new DateOnly(1978, 11, 3)),
      new PatientDefinition("DEMO-PAT-003", "Priya", "Ellison", new DateOnly(1986, 6, 25)),
      new PatientDefinition("DEMO-PAT-004", "Callum", "Redford", new DateOnly(1959, 2, 17)),
      new PatientDefinition("DEMO-PAT-005", "Nia", "Pembroke", new DateOnly(2001, 9, 8)),
      new PatientDefinition("DEMO-PAT-006", "Ellis", "Bramwell", new DateOnly(2012, 1, 30)),
      new PatientDefinition("DEMO-PAT-007", "Zara", "Whitcombe", new DateOnly(1947, 12, 14)),
      new PatientDefinition("DEMO-PAT-008", "Theo", "Langford", new DateOnly(1996, 5, 19)),
      new PatientDefinition("DEMO-PAT-009", "Maya", "Fenwick", new DateOnly(1982, 8, 27)),
      new PatientDefinition("DEMO-PAT-010", "Idris", "Calder", new DateOnly(1968, 3, 6)),
      new PatientDefinition("DEMO-PAT-011", "Freya", "Northcott", new DateOnly(2007, 10, 22)),
      new PatientDefinition("DEMO-PAT-012", "Rowan", "Beckett", new DateOnly(1973, 7, 11))
    };

    return definitions
        .Select((definition, index) =>
        {
          var patient = new Patient(
              definition.Reference,
              definition.FirstName,
              definition.LastName,
              definition.DateOfBirth);

          FixtureStateHydrator.Set(
              patient,
              nameof(Patient.Id),
              StableDemoGuid.For(definition.Reference));
          FixtureStateHydrator.Set(
              patient,
              nameof(Patient.CreatedAt),
              anchor.AddDays(-90 + (index * 5)));

          return patient;
        })
        .ToArray();
  }

  private static IReadOnlyList<Referral> CreateReferrals(
      DateTime anchor,
      IReadOnlyDictionary<string, Patient> patients)
  {
    var definitions = new[]
    {
      new ReferralDefinition("DEMO-REF-001", "DEMO-PAT-001", ReferralPriority.Urgent, ReferralStatus.AwaitingTriage, "Synthetic demo: urgent review requested for a newly reported change in symptoms."),
      new ReferralDefinition("DEMO-REF-002", "DEMO-PAT-002", ReferralPriority.Routine, ReferralStatus.AwaitingTriage, "Synthetic demo: routine assessment requested to clarify an ongoing care pathway.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-003", "DEMO-PAT-003", ReferralPriority.Routine, ReferralStatus.Accepted, "Synthetic demo: referral accepted and ready for a clinical team assignment.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-004", "DEMO-PAT-004", ReferralPriority.Urgent, ReferralStatus.Accepted, "Synthetic demo: urgent referral accepted after review and awaiting assignment.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-005", "DEMO-PAT-005", ReferralPriority.Urgent, ReferralStatus.Assigned, "Synthetic demo: referral assigned for coordinated clinical review.", RecordAssessment: true, AssignedTo: "Cardiology Team A"),
      new ReferralDefinition("DEMO-REF-006", "DEMO-PAT-006", ReferralPriority.Routine, ReferralStatus.Assigned, "Synthetic demo: routine referral assigned and ready to be scheduled.", RecordAssessment: true, AssignedTo: "Respiratory Team"),
      new ReferralDefinition("DEMO-REF-007", "DEMO-PAT-007", ReferralPriority.Routine, ReferralStatus.Scheduled, "Synthetic demo: scheduled consultation with a prior cancelled booking retained in the journey.", RecordAssessment: true, AssignedTo: "Cardiology Team B"),
      new ReferralDefinition("DEMO-REF-008", "DEMO-PAT-008", ReferralPriority.Urgent, ReferralStatus.Scheduled, "Synthetic demo: urgent diagnostic pathway with current and follow-up appointments.", RecordAssessment: true, AssignedTo: "Cardiology Team A", InitialAssignedTo: "Cardiology Team B"),
      new ReferralDefinition("DEMO-REF-009", "DEMO-PAT-009", ReferralPriority.Routine, ReferralStatus.Scheduled, "Synthetic demo: scheduling pathway showing cancelled and did-not-attend outcomes.", RecordAssessment: true, AssignedTo: "Respiratory Team"),
      new ReferralDefinition("DEMO-REF-010", "DEMO-PAT-010", ReferralPriority.Urgent, ReferralStatus.InProgress, "Synthetic demo: active clinical workflow ready for notes and appointment completion.", RecordAssessment: true, AssignedTo: "Cardiology Team A"),
      new ReferralDefinition("DEMO-REF-011", "DEMO-PAT-011", ReferralPriority.Routine, ReferralStatus.InProgress, "Synthetic demo: completed appointment awaiting explicit referral completion.", RecordAssessment: true, AssignedTo: "Cardiology Team B"),
      new ReferralDefinition("DEMO-REF-012", "DEMO-PAT-012", ReferralPriority.Routine, ReferralStatus.Completed, "Synthetic demo: fully completed historical journey with follow-up documentation.", RecordAssessment: true, AssignedTo: "Respiratory Team"),
      new ReferralDefinition("DEMO-REF-013", "DEMO-PAT-001", ReferralPriority.Urgent, ReferralStatus.Rejected, "Synthetic demo: separate referral rejected after a documented triage review.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-014", "DEMO-PAT-002", ReferralPriority.Routine, ReferralStatus.MoreInformationRequired, "Synthetic demo: additional non-sensitive information requested before a decision.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-015", "DEMO-PAT-003", ReferralPriority.Urgent, ReferralStatus.MoreInformationRequired, "Synthetic demo: urgent referral paused while clarification is requested.", RecordAssessment: true),
      new ReferralDefinition("DEMO-REF-016", "DEMO-PAT-004", ReferralPriority.Routine, ReferralStatus.Draft, "Synthetic demo: draft referral available for submission."),
      new ReferralDefinition("DEMO-REF-017", "DEMO-PAT-005", ReferralPriority.Urgent, ReferralStatus.Submitted, "Synthetic demo: submitted referral ready to enter triage.")
    };

    return definitions
        .Select((definition, index) => CreateReferral(
            definition,
            patients[definition.PatientReference],
            anchor.AddDays(-60 + (index * 3))))
        .ToArray();
  }

  private static Referral CreateReferral(
      ReferralDefinition definition,
      Patient patient,
      DateTime createdAt)
  {
    var referral = new Referral(
        definition.Reference,
        patient.Id,
        definition.Priority,
        definition.Reason);

    FixtureStateHydrator.Set(
        referral,
        nameof(Referral.Id),
        StableDemoGuid.For(definition.Reference));
    FixtureStateHydrator.Set(
        referral.History.Single(),
        nameof(ReferralHistoryEntry.ReferralId),
        referral.Id);

    if (definition.Status != ReferralStatus.Draft)
    {
      referral.Submit();
    }

    if (definition.Status is not ReferralStatus.Draft and not ReferralStatus.Submitted)
    {
      referral.StartTriage();
    }

    if (definition.RecordAssessment)
    {
      referral.RecordTriageAssessment(
          definition.Priority,
          "Synthetic demo triage assessment recorded for workflow demonstration only.");
    }

    switch (definition.Status)
    {
      case ReferralStatus.Draft:
      case ReferralStatus.Submitted:
      case ReferralStatus.AwaitingTriage:
        break;
      case ReferralStatus.MoreInformationRequired:
        referral.RequestMoreInformation();
        break;
      case ReferralStatus.Rejected:
        referral.Reject();
        break;
      default:
        referral.Accept();

        if (definition.Status == ReferralStatus.Accepted)
        {
          break;
        }

        referral.Assign(
            definition.InitialAssignedTo ?? definition.AssignedTo
            ?? throw new InvalidOperationException("Assigned demo referral requires a team."));

        if (definition.InitialAssignedTo is not null)
        {
          referral.Reassign(
              definition.AssignedTo
              ?? throw new InvalidOperationException("Reassigned demo referral requires a final team."));
        }

        if (definition.Status == ReferralStatus.Assigned)
        {
          break;
        }

        referral.Schedule();

        if (definition.Status == ReferralStatus.Scheduled)
        {
          break;
        }

        referral.StartProgress();

        if (definition.Status == ReferralStatus.Completed)
        {
          referral.Complete();
        }

        break;
    }

    NormalizeReferralState(
        referral,
        createdAt);

    return referral;
  }

}
