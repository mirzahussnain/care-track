namespace CareTrack.Application.Common.Models;

public sealed record CreateClinicalNoteResult(
    Guid Id,
    Guid AppointmentId,
    string Content,
    string CreatedBy,
    DateTime CreatedAt,
    DateTime? UpdatedAt);