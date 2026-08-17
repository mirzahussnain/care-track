using System.ComponentModel.DataAnnotations;

namespace CareTrack.Api.Contracts.ClinicalNotes;

public sealed record UpdateClinicalNoteRequest(
    [Required]
    [MaxLength(5000)]
    string Content);