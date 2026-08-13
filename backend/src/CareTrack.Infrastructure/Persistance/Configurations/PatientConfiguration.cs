using CareTrack.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CareTrack.Infrastructure.Persistance.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
  public void Configure(EntityTypeBuilder<Patient> builder)
  {
    builder.ToTable("Patients");
    builder.HasKey(patient => patient.Id);
    builder.Property(patient => patient.PatientReference).IsRequired().HasMaxLength(20);
    builder.HasIndex(patient => patient.PatientReference).IsUnique();
    builder.Property(patient => patient.FirstName).IsRequired().HasMaxLength(100);
    builder.Property(patient => patient.LastName).IsRequired().HasMaxLength(100);
    builder.Property(patient => patient.DateOfBirth).IsRequired();
    builder.Property(patient => patient.CreatedAt).IsRequired();
    builder.Property(patient => patient.RowVersion).IsRowVersion();
  }
}