
namespace CareTrack.Domain.Entities;

public class Patient
{
  public Guid Id { get; private set; }
  public string PatientReference { get; private set; }
  public string FirstName { get; private set; }

  public string LastName { get; private set; }

  public DateOnly DateOfBirth { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public string FullName => $"{FirstName} {LastName}";

  public byte[] RowVersion { get; private set; }
  public Patient(
  string patientReference,
  string firstName,
  string lastName,
  DateOnly dateOfBirth)
  {
    if (string.IsNullOrWhiteSpace(patientReference))
      throw new ArgumentException(
      "Patient reference is required.",
        nameof(patientReference));
    if (string.IsNullOrWhiteSpace(firstName))
      throw new ArgumentException(
          "First name is required.",
          nameof(firstName));
    if (string.IsNullOrWhiteSpace(lastName))
      throw new ArgumentException(
          "Last name is required.",
          nameof(lastName));
    if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
      throw new ArgumentException(
          "Date of birth cannot be in the future.",
          nameof(dateOfBirth));

    Id = Guid.NewGuid();
    PatientReference = patientReference.Trim();
    FirstName = firstName.Trim();
    LastName = lastName.Trim();
    DateOfBirth = dateOfBirth;
    CreatedAt = DateTime.UtcNow;
  }


  public void UpdateName(string firstName, string lastName)
  {
    if (string.IsNullOrWhiteSpace(firstName)) throw new ArgumentException("First name is required.", nameof(firstName));
    if (string.IsNullOrWhiteSpace(lastName)) throw new ArgumentException("Last name is required.", nameof(lastName));

    FirstName = firstName.Trim();
    LastName = lastName.Trim();
  }

  public void UpdateDateOfBirth(DateOnly dateOfBirth)
  {
    if (dateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow)) throw new ArgumentException("Date of birth cannot be in the future.", nameof(dateOfBirth));
    DateOfBirth = dateOfBirth;
  }


}