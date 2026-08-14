using System.ComponentModel.DataAnnotations;

namespace CareTrack.Api.Contracts.Referrals;

public sealed record AssignReferralRequest(
    [Required]
    [StringLength(200)]
    string AssignedTo);