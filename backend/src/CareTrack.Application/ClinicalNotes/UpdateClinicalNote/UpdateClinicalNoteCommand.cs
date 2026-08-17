namespace CareTrack.Application.ClinicalNotes.UpdateClinicalNote;

public sealed record UpdateClinicalNoteCommand(
    Guid Id,
    string Content);