using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public sealed class ReferralConfiguration
    : IEntityTypeConfiguration<Referral>
{
  public void Configure(
      EntityTypeBuilder<Referral> builder)
  {
    builder.ToTable("Referrals");

    builder.HasKey(
        referral => referral.Id);

    builder.Property(
            referral =>
                referral.ReferralReference)
        .IsRequired()
        .HasMaxLength(30);

    builder.HasIndex(
            referral =>
                referral.ReferralReference)
        .IsUnique();

    builder.Property(
            referral => referral.Reason)
        .IsRequired()
        .HasMaxLength(2000);

    builder.Property(
            referral => referral.Status)
        .IsRequired();

    builder.Property(
            referral => referral.Priority)
        .IsRequired();

    builder.Property(
            referral => referral.CreatedAt)
        .IsRequired();

    builder.Property(
        referral => referral.SubmittedAt);

    builder.Property(
        referral => referral.UpdatedAt);


    builder.HasOne<Patient>().WithMany().HasForeignKey(referral => referral.PatientId).OnDelete(DeleteBehavior.Restrict);
    builder.Property(referral => referral.TriageNote).HasMaxLength(2000);
    builder.Property(referral => referral.TriagedAt);
    builder.Property(referral => referral.AssignedTo).HasMaxLength(200);
    builder.Property(referral => referral.AssignedAt);

  }


}