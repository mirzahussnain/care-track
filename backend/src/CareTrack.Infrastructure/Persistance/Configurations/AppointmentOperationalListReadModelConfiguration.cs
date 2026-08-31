using CareTrack.Infrastructure.Persistance.ReadModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public sealed class AppointmentOperationalListReadModelConfiguration
    : IEntityTypeConfiguration<AppointmentOperationalListReadModel>
{
  public void Configure(
      EntityTypeBuilder<AppointmentOperationalListReadModel> builder)
  {
    builder.HasNoKey();
    builder.ToView("vw_AppointmentOperationalList", "dbo");

    builder.Property(row => row.AppointmentReference).HasMaxLength(30);
    builder.Property(row => row.Location).HasMaxLength(200);
    builder.Property(row => row.PatientReference).HasMaxLength(20);
    builder.Property(row => row.PatientDisplayName).HasMaxLength(201);
    builder.Property(row => row.ReferralReference).HasMaxLength(30);
  }
}
