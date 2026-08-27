using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

namespace CareTrack.DemoSeeder;

public static partial class DemoDatasetFactory
{
  private static void NormalizeReferralState(
      Referral referral,
      DateTime createdAt)
  {
    var history = referral.History.ToArray();

    for (var index = 0; index < history.Length; index++)
    {
      FixtureStateHydrator.Set(
          history[index],
          nameof(ReferralHistoryEntry.Id),
          StableDemoGuid.For($"{referral.ReferralReference}:history:{index + 1}"));
      FixtureStateHydrator.Set(
          history[index],
          nameof(ReferralHistoryEntry.ReferralId),
          referral.Id);
      FixtureStateHydrator.Set(
          history[index],
          nameof(ReferralHistoryEntry.OccurredAt),
          createdAt.AddHours(index * 6));
    }

    FixtureStateHydrator.Set(
        referral,
        nameof(Referral.CreatedAt),
        createdAt);

    var submitted = history.FirstOrDefault(entry =>
        entry.EventType == ReferralHistoryEventType.Submitted);
    var triaged = history.LastOrDefault(entry =>
        entry.EventType == ReferralHistoryEventType.TriageAssessmentRecorded);
    var assigned = history.LastOrDefault(entry =>
        entry.EventType is ReferralHistoryEventType.Assigned or ReferralHistoryEventType.Reassigned);

    FixtureStateHydrator.Set<DateTime?>(
        referral,
        nameof(Referral.SubmittedAt),
        submitted?.OccurredAt);
    FixtureStateHydrator.Set<DateTime?>(
        referral,
        nameof(Referral.TriagedAt),
        triaged?.OccurredAt);
    FixtureStateHydrator.Set<DateTime?>(
        referral,
        nameof(Referral.AssignedAt),
        assigned?.OccurredAt);
    FixtureStateHydrator.Set<DateTime?>(
        referral,
        nameof(Referral.UpdatedAt),
        history.Length > 1 ? history[^1].OccurredAt : null);
  }

  private static IReadOnlyList<Appointment> CreateAppointments(
      DateTime anchor,
      IReadOnlyDictionary<string, Referral> referrals)
  {
    var definitions = new[]
    {
      new AppointmentDefinition("DEMO-APT-001", "DEMO-REF-007", AppointmentType.Consultation, anchor.AddDays(1).AddHours(-3), 45, "CareTrack Demo Clinic - Room 1", AppointmentStatus.Scheduled),
      new AppointmentDefinition("DEMO-APT-002", "DEMO-REF-007", AppointmentType.Consultation, anchor.AddDays(-2).AddHours(-3), 45, "CareTrack Demo Clinic - Room 1", AppointmentStatus.Cancelled),
      new AppointmentDefinition("DEMO-APT-003", "DEMO-REF-008", AppointmentType.Diagnostic, anchor.AddHours(-2), 30, "CareTrack Demo Clinic - Diagnostics", AppointmentStatus.CheckedIn),
      new AppointmentDefinition("DEMO-APT-004", "DEMO-REF-008", AppointmentType.FollowUp, anchor.AddDays(4).AddHours(2), 30, "CareTrack Demo Clinic - Room 2", AppointmentStatus.Scheduled),
      new AppointmentDefinition("DEMO-APT-005", "DEMO-REF-009", AppointmentType.Consultation, anchor.AddDays(-1).AddHours(-1), 45, "CareTrack Demo Clinic - Room 3", AppointmentStatus.Cancelled),
      new AppointmentDefinition("DEMO-APT-006", "DEMO-REF-009", AppointmentType.FollowUp, anchor.AddDays(-5).AddHours(3), 30, "CareTrack Demo Clinic - Room 3", AppointmentStatus.DidNotAttend),
      new AppointmentDefinition("DEMO-APT-007", "DEMO-REF-010", AppointmentType.Procedure, anchor.AddHours(-1), 60, "CareTrack Demo Clinic - Procedure Room", AppointmentStatus.InProgress),
      new AppointmentDefinition("DEMO-APT-008", "DEMO-REF-011", AppointmentType.Consultation, anchor.AddDays(-3).AddHours(1), 45, "CareTrack Demo Clinic - Room 2", AppointmentStatus.Completed),
      new AppointmentDefinition("DEMO-APT-009", "DEMO-REF-012", AppointmentType.Diagnostic, anchor.AddDays(-21).AddHours(-3), 45, "CareTrack Demo Clinic - Diagnostics", AppointmentStatus.Completed),
      new AppointmentDefinition("DEMO-APT-010", "DEMO-REF-012", AppointmentType.FollowUp, anchor.AddDays(-7).AddHours(-2), 30, "CareTrack Demo Clinic - Room 1", AppointmentStatus.Completed)
    };

    return definitions
        .Select(definition => CreateAppointment(
            definition,
            referrals[definition.ReferralReference]))
        .ToArray();
  }

  private static Appointment CreateAppointment(
      AppointmentDefinition definition,
      Referral referral)
  {
    var appointment = new Appointment(
        definition.Reference,
        referral.PatientId,
        referral.Id,
        definition.Type,
        definition.Start,
        definition.Start.AddMinutes(definition.DurationMinutes),
        definition.Location);

    FixtureStateHydrator.Set(
        appointment,
        nameof(Appointment.Id),
        StableDemoGuid.For(definition.Reference));

    switch (definition.Status)
    {
      case AppointmentStatus.Scheduled:
        break;
      case AppointmentStatus.CheckedIn:
        appointment.CheckIn();
        break;
      case AppointmentStatus.InProgress:
        appointment.CheckIn();
        appointment.Start(definition.Start.AddMinutes(10));
        break;
      case AppointmentStatus.Completed:
        appointment.CheckIn();
        appointment.Start(definition.Start.AddMinutes(5));
        appointment.Complete();
        break;
      case AppointmentStatus.Cancelled:
        appointment.Cancel();
        break;
      case AppointmentStatus.DidNotAttend:
        appointment.MarkDidNotAttend();
        break;
      default:
        throw new InvalidOperationException(
            $"Unsupported demo appointment status '{definition.Status}'.");
    }

    NormalizeAppointmentState(
        appointment,
        definition.Status);

    return appointment;
  }

  private static void NormalizeAppointmentState(
      Appointment appointment,
      AppointmentStatus status)
  {
    var createdAt = appointment.ScheduledStart.AddDays(-7);
    var checkedInAt = appointment.ScheduledStart.AddMinutes(-10);
    var startedAt = appointment.ScheduledStart.AddMinutes(5);
    var completedAt = appointment.ScheduledEnd.AddMinutes(5);
    var outcomeAt = appointment.ScheduledStart.AddHours(-2);

    FixtureStateHydrator.Set(
        appointment,
        nameof(Appointment.CreatedAt),
        createdAt);
    FixtureStateHydrator.Set<DateTime?>(
        appointment,
        nameof(Appointment.CheckedInAt),
        status is AppointmentStatus.CheckedIn or AppointmentStatus.InProgress or AppointmentStatus.Completed
            ? checkedInAt
            : null);
    FixtureStateHydrator.Set<DateTime?>(
        appointment,
        nameof(Appointment.StartedAt),
        status is AppointmentStatus.InProgress or AppointmentStatus.Completed
            ? startedAt
            : null);
    FixtureStateHydrator.Set<DateTime?>(
        appointment,
        nameof(Appointment.CompletedAt),
        status == AppointmentStatus.Completed
            ? completedAt
            : null);
    FixtureStateHydrator.Set<DateTime?>(
        appointment,
        nameof(Appointment.CancelledAt),
        status == AppointmentStatus.Cancelled
            ? outcomeAt
            : null);
    FixtureStateHydrator.Set<DateTime?>(
        appointment,
        nameof(Appointment.DidNotAttendAt),
        status == AppointmentStatus.DidNotAttend
            ? appointment.ScheduledEnd.AddMinutes(15)
            : null);

    var updatedAt = status switch
    {
      AppointmentStatus.Scheduled => (DateTime?)null,
      AppointmentStatus.CheckedIn => checkedInAt,
      AppointmentStatus.InProgress => startedAt,
      AppointmentStatus.Completed => completedAt,
      AppointmentStatus.Cancelled => outcomeAt,
      AppointmentStatus.DidNotAttend => appointment.ScheduledEnd.AddMinutes(15),
      _ => null
    };

    FixtureStateHydrator.Set(
        appointment,
        nameof(Appointment.UpdatedAt),
        updatedAt);
  }

  private static IReadOnlyList<ClinicalNote> CreateClinicalNotes(
      DateTime anchor,
      IReadOnlyDictionary<string, Appointment> appointments)
  {
    var definitions = new[]
    {
      new ClinicalNoteDefinition("DEMO-NOTE-001", "DEMO-APT-007", "Symptoms reviewed during the demonstration appointment. Follow-up plan discussed.", anchor.AddHours(-1)),
      new ClinicalNoteDefinition("DEMO-NOTE-002", "DEMO-APT-008", "Demo clinical review completed. No real patient information is represented in this record.", anchor.AddDays(-3).AddHours(2)),
      new ClinicalNoteDefinition("DEMO-NOTE-003", "DEMO-APT-008", "Synthetic follow-up actions recorded for the portfolio workflow demonstration.", anchor.AddDays(-3).AddHours(3)),
      new ClinicalNoteDefinition("DEMO-NOTE-004", "DEMO-APT-009", "Demonstration diagnostic discussion completed using synthetic information only.", anchor.AddDays(-21).AddHours(-2)),
      new ClinicalNoteDefinition("DEMO-NOTE-005", "DEMO-APT-009", "Synthetic outcome reviewed and a demonstration follow-up was arranged.", anchor.AddDays(-21).AddHours(-1)),
      new ClinicalNoteDefinition("DEMO-NOTE-006", "DEMO-APT-010", "Follow-up review completed for this entirely synthetic CareTrack record.", anchor.AddDays(-7).AddHours(-1)),
      new ClinicalNoteDefinition("DEMO-NOTE-007", "DEMO-APT-010", "Demo pathway complete. No real clinical or personal data is contained in this note.", anchor.AddDays(-7))
    };

    return definitions
        .Select(definition =>
        {
          var note = new ClinicalNote(
              appointments[definition.AppointmentReference].Id,
              definition.Content,
              ClinicianObjectId);

          FixtureStateHydrator.Set(
              note,
              nameof(ClinicalNote.Id),
              StableDemoGuid.For(definition.Reference));
          FixtureStateHydrator.Set(
              note,
              nameof(ClinicalNote.CreatedAt),
              definition.CreatedAt);

          return note;
        })
        .ToArray();
  }

  private sealed record PatientDefinition(
      string Reference,
      string FirstName,
      string LastName,
      DateOnly DateOfBirth);

  private sealed record ReferralDefinition(
      string Reference,
      string PatientReference,
      ReferralPriority Priority,
      ReferralStatus Status,
      string Reason,
      bool RecordAssessment = false,
      string? AssignedTo = null,
      string? InitialAssignedTo = null);

  private sealed record AppointmentDefinition(
      string Reference,
      string ReferralReference,
      AppointmentType Type,
      DateTime Start,
      int DurationMinutes,
      string Location,
      AppointmentStatus Status);

  private sealed record ClinicalNoteDefinition(
      string Reference,
      string AppointmentReference,
      string Content,
      DateTime CreatedAt);
}
