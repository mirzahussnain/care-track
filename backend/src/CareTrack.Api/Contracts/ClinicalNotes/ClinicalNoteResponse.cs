namespace CareTrack.Api.Contracts.ClinicalNotes;

public sealed record ClinicalNoteResponse(
    Guid Id,
    Guid AppointmentId,
    string Content,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);