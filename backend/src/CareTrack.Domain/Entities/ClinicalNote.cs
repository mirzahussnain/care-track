namespace CareTrack.Domain.Entities;

public class ClinicalNote
{
  public Guid Id { get; private set; }

  public Guid AppointmentId { get; private set; }

  public string Content { get; private set; }

  public string CreatedBy { get; private set; }

  public DateTime CreatedAt { get; private set; }

  public DateTime? UpdatedAt { get; private set; }

  private ClinicalNote()
  {
    Content = null;
    CreatedBy = null;
  }
  public ClinicalNote(
  Guid appointmentId,
  string content,
  string createdBy


  )
  {
    if (appointmentId == Guid.Empty)
    {
      throw new ArgumentException(
                     "Appointment ID cannot be empty.",
                     nameof(appointmentId));
    }

    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException(
          "Clinical note content is required.",
          nameof(content));
    }

    if (content.Trim().Length > 5000)
    {
      throw new ArgumentException(
          "Clinical note content cannot exceed 5000 characters.",
          nameof(content));
    }

    if (string.IsNullOrWhiteSpace(createdBy))
    {
      throw new ArgumentException(
          "Clinical note author is required.",
          nameof(createdBy));
    }

    if (createdBy.Trim().Length > 200)
    {
      throw new ArgumentException(
          "Clinical note author cannot exceed 200 characters.",
          nameof(createdBy));
    }

    Id = Guid.NewGuid();

    AppointmentId = appointmentId;

    Content = content.Trim();

    CreatedBy = createdBy.Trim();

    CreatedAt = DateTime.UtcNow;

  }

  public void UpdateContent(
    string content)
  {
    if (string.IsNullOrWhiteSpace(content))
    {
      throw new ArgumentException(
          "Clinical note content is required.",
          nameof(content));
    }

    if (content.Trim().Length > 5000)
    {
      throw new ArgumentException(
          "Clinical note content cannot exceed 5000 characters.",
          nameof(content));
    }

    Content = content.Trim();

    UpdatedAt = DateTime.UtcNow;
  }
}