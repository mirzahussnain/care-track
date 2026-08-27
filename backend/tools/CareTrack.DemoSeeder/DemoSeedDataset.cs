using CareTrack.Domain.Entities;

namespace CareTrack.DemoSeeder;

public sealed record DemoSeedDataset(
    IReadOnlyList<Patient> Patients,
    IReadOnlyList<Referral> Referrals,
    IReadOnlyList<Appointment> Appointments,
    IReadOnlyList<ClinicalNote> ClinicalNotes)
{
  public int ReferralHistoryCount =>
      Referrals.Sum(referral => referral.History.Count);
}

public sealed record DemoSeedCounts(
    int Patients,
    int Referrals,
    int ReferralHistoryEntries,
    int Appointments,
    int ClinicalNotes);
