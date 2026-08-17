using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public sealed class ClinicalNoteConfiguration : IEntityTypeConfiguration<ClinicalNote>
{
  public void Configure(EntityTypeBuilder<ClinicalNote> builder)
  {
    builder.ToTable("ClinicalNotes");
    builder.HasKey(note => note.Id);
    builder.Property(note => note.AppointmentId).IsRequired();
    builder.Property(note => note.Content).IsRequired().HasMaxLength(5000);
    builder.Property(note => note.CreatedBy).IsRequired().HasMaxLength(200);

    builder.Property(note => note.CreatedAt).IsRequired();
    builder.Property(note => note.UpdatedAt);
    builder.HasIndex(note => note.AppointmentId);

    builder.HasOne<Appointment>()
        .WithMany()
        .HasForeignKey(
            note => note.AppointmentId)
        .OnDelete(
            DeleteBehavior.Restrict);
  }
}