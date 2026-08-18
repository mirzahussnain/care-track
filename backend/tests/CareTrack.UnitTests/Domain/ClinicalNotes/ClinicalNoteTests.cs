using CareTrack.Domain.Entities;
namespace CareTrack.UnitTests.Domain.ClinicalNotes;

public class ClinicalNoteTests
{
  [Fact]
  public void Constructor_WithValidValues_CreatesClinicalNote()
  {
    // Arrange
    var appointmentId =
        Guid.NewGuid();

    // Act
    var note =
        new ClinicalNote(
            appointmentId,
            "Patient reports improvement in symptoms.",
            "clinician.demo");

    // Assert
    Assert.NotEqual(
        Guid.Empty,
        note.Id);

    Assert.Equal(
        appointmentId,
        note.AppointmentId);

    Assert.Equal(
        "Patient reports improvement in symptoms.",
        note.Content);

    Assert.Equal(
        "clinician.demo",
        note.CreatedBy);

    Assert.NotEqual(
        default,
        note.CreatedAt);

    Assert.Null(
        note.UpdatedAt);
  }

  [Fact]
  public void Constructor_WhenAppointmentIdIsEmpty_ThrowsArgumentException()
  {
    // Act
    var action =
        () => new ClinicalNote(
            Guid.Empty,
            "Clinical note",
            "clinician.demo");

    // Assert
    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenContentIsBlank_ThrowsArgumentException()
  {
    var action =
        () => new ClinicalNote(
            Guid.NewGuid(),
            "   ",
            "clinician.demo");

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenContentExceedsMaximum_ThrowsArgumentException()
  {
    var content =
        new string('a', 5001);

    var action =
        () => new ClinicalNote(
            Guid.NewGuid(),
            content,
            "clinician.demo");

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenCreatedByIsBlank_ThrowsArgumentException()
  {
    var action =
        () => new ClinicalNote(
            Guid.NewGuid(),
            "Clinical note",
            "   ");

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenCreatedByExceedsMaximum_ThrowsArgumentException()
  {
    var createdBy =
        new string('a', 201);

    var action =
        () => new ClinicalNote(
            Guid.NewGuid(),
            "Clinical note",
            createdBy);

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_TrimsContentAndCreatedBy()
  {
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "  Clinical note text  ",
            "  clinician.demo  ");

    Assert.Equal(
        "Clinical note text",
        note.Content);

    Assert.Equal(
        "clinician.demo",
        note.CreatedBy);
  }

  [Fact]
  public void UpdateContent_WithValidContent_UpdatesContentAndTimestamp()
  {
    // Arrange
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original note",
            "clinician.demo");

    // Act
    note.UpdateContent(
        "Updated note");

    // Assert
    Assert.Equal(
        "Updated note",
        note.Content);

    Assert.NotNull(
        note.UpdatedAt);
  }

  [Fact]
  public void UpdateContent_TrimsContent()
  {
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    note.UpdateContent(
        "  Updated content  ");

    Assert.Equal(
        "Updated content",
        note.Content);
  }

  [Fact]
  public void UpdateContent_WhenBlank_ThrowsArgumentException()
  {
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    var action =
        () => note.UpdateContent("   ");

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void UpdateContent_WhenContentExceedsMaximum_ThrowsArgumentException()
  {
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    var action =
        () => note.UpdateContent(
            new string('a', 5001));

    Assert.Throws<ArgumentException>(
        action);
  }

  [Fact]
  public void UpdateContent_DoesNotChangeCreatedBy()
  {
    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    note.UpdateContent(
        "Updated");

    Assert.Equal(
        "clinician.demo",
        note.CreatedBy);
  }
}