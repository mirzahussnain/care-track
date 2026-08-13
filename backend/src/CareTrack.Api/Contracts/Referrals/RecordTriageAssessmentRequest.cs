using System.ComponentModel.DataAnnotations;
using CareTrack.Domain.Enums;

namespace CareTrack.Api.Contracts.Referrals;

public sealed record RecordTriageAssessmentRequest(
    ReferralPriority Priority,

    [Required]
    [StringLength(2000)]
    string Note);