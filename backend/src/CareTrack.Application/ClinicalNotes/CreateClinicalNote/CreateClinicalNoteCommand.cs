namespace CareTrack.Application.ClinicalNotes.CreateClinicalNote;

public sealed record CreateClinicalNoteCommand(
    Guid AppointmentId,
    string Content);