using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public class AppointmentConfiguration
    : IEntityTypeConfiguration<Appointment>
{
  public void Configure(
      EntityTypeBuilder<Appointment> builder)
  {
    builder.ToTable("Appointments");

    builder.HasKey(
        appointment =>
            appointment.Id);

    builder.Property(
            appointment =>
                appointment.AppointmentReference)
        .IsRequired()
        .HasMaxLength(30);

    builder.HasIndex(
            appointment =>
                appointment.AppointmentReference)
        .IsUnique();

    builder.Property(
            appointment =>
                appointment.PatientId)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.ReferralId)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.AppointmentType)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.ScheduledStart)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.ScheduledEnd)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.Location)
        .IsRequired()
        .HasMaxLength(200);

    builder.Property(
            appointment =>
                appointment.Status)
        .IsRequired();

    builder.Property(
            appointment =>
                appointment.CreatedAt)
        .IsRequired();

    builder.Property(
        appointment =>
            appointment.UpdatedAt);

    builder.HasIndex(
        appointment =>
            appointment.PatientId);

    builder.HasIndex(
        appointment =>
            appointment.ReferralId);

    builder.HasOne<Patient>()
    .WithMany()
    .HasForeignKey(
        appointment =>
            appointment.PatientId)
    .OnDelete(
        DeleteBehavior.Restrict);

    builder.HasOne<Referral>()
        .WithMany()
        .HasForeignKey(
            appointment =>
                appointment.ReferralId)
        .OnDelete(
            DeleteBehavior.Restrict);
  }
}