using System.ComponentModel.DataAnnotations;

namespace CareTrack.Api.Contracts.ClinicalNotes;

public sealed record CreateClinicalNoteRequest(
    [Required]
    [MaxLength(5000)]
    string Content);