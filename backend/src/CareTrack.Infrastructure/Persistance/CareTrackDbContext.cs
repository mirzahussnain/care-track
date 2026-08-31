using CareTrack.Domain.Entities;
using CareTrack.Infrastructure.Persistance.ReadModels;
using Microsoft.EntityFrameworkCore;

namespace CareTrack.Infrastructure.Persistance;

public class CareTrackDbContext : DbContext
{
  public CareTrackDbContext(
  DbContextOptions<CareTrackDbContext> options
  ) : base(options)
  {

  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(CareTrackDbContext).Assembly);
  }

  public DbSet<Patient> Patients => Set<Patient>();
  public DbSet<Referral> Referrals => Set<Referral>();
  public DbSet<ReferralHistoryEntry> ReferralHistoryEntries => Set<ReferralHistoryEntry>();
  public DbSet<Appointment> Appointments => Set<Appointment>();
  public DbSet<ClinicalNote> ClinicalNotes => Set<ClinicalNote>();
  public DbSet<AppointmentOperationalListReadModel> AppointmentOperationalList =>
      Set<AppointmentOperationalListReadModel>();
}
