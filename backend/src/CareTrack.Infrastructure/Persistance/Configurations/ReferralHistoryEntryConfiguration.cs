using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public sealed class ReferralHistoryEntryConfiguration
    : IEntityTypeConfiguration<ReferralHistoryEntry>
{
  public void Configure(
      EntityTypeBuilder<ReferralHistoryEntry> builder)
  {
    builder.ToTable(
        "ReferralHistoryEntries");

    builder.HasKey(
        history => history.Id);

    builder.Property(
        history => history.Id)
    .ValueGeneratedNever();

    builder.Property(
            history => history.EventType)
        .IsRequired();

    builder.Property(
        history => history.FromStatus);

    builder.Property(
        history => history.ToStatus);

    builder.Property(
        history => history.Priority);

    builder.Property(
            history => history.TriageNote)
        .HasMaxLength(2000);

    builder.Property(
            history => history.AssignedTo)
        .HasMaxLength(200);

    builder.Property(
            history => history.OccurredAt)
        .IsRequired();

    builder.HasIndex(
        history => new
        {
          history.ReferralId,
          history.OccurredAt
        });
  }
}