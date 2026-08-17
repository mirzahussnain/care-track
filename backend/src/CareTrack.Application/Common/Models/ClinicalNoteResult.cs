namespace CareTrack.Application.ClinicalNotes.Common;

public sealed record ClinicalNoteResult(
    Guid Id,
    Guid AppointmentId,
    string Content,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);