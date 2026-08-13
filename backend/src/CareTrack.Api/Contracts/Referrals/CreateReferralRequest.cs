using System.ComponentModel.DataAnnotations;
using CareTrack.Domain.Enums;
namespace CareTrack.Api.Contracts.Referrals;

public sealed record CreateReferralRequest
(
   [Required]
    [StringLength(30)]
    string ReferralReference,

    Guid PatientId,

    ReferralPriority Priority,

    [Required]
    [StringLength(2000)]
    string Reason
);