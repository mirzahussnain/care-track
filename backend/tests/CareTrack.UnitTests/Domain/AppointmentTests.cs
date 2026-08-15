using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;

namespace CareTrack.UnitTests.Domain;

public class AppointmentTests
{
  [Fact]
  public void Constructor_WithValidValues_CreatesScheduledAppointment()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referralId =
        Guid.NewGuid();

    var start =
        DateTime.UtcNow.AddDays(2);

    var end =
        start.AddMinutes(30);

    // Act
    var appointment =
        new Appointment(
            "APT-001",
            patientId,
            referralId,
            AppointmentType.Consultation,
            start,
            end,
            "Birmingham Clinic");

    // Assert
    Assert.NotEqual(
        Guid.Empty,
        appointment.Id);

    Assert.Equal(
        "APT-001",
        appointment.AppointmentReference);

    Assert.Equal(
        patientId,
        appointment.PatientId);

    Assert.Equal(
        referralId,
        appointment.ReferralId);

    Assert.Equal(
        AppointmentType.Consultation,
        appointment.AppointmentType);

    Assert.Equal(
        start,
        appointment.ScheduledStart);

    Assert.Equal(
        end,
        appointment.ScheduledEnd);

    Assert.Equal(
        "Birmingham Clinic",
        appointment.Location);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        appointment.Status);

    Assert.NotEqual(
        default,
        appointment.CreatedAt);

    Assert.Null(
        appointment.UpdatedAt);
  }

  [Fact]
  public void Constructor_WithBlankReference_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            " ",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WithEmptyPatientId_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.Empty,
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }
  [Fact]
  public void Constructor_WithEmptyReferralId_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.Empty,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WithInvalidAppointmentType_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    var invalidType =
        (AppointmentType)999;

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            invalidType,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenScheduledEndIsBeforeStart_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    var end =
        start.AddMinutes(-30);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            end,
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenScheduledEndEqualsStart_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start,
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WithBlankLocation_ThrowsArgumentException()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            " ");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_TrimsReferenceAndLocation()
  {
    // Arrange
    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var appointment =
        new Appointment(
            "  APT-001  ",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "  Birmingham Clinic  ");

    // Assert
    Assert.Equal(
        "APT-001",
        appointment.AppointmentReference);

    Assert.Equal(
        "Birmingham Clinic",
        appointment.Location);
  }

  [Fact]
  public void Constructor_WhenReferenceExceedsMaximumLength_ThrowsArgumentException()
  {
    // Arrange
    var reference =
        new string('A', 31);

    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            reference,
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

  [Fact]
  public void Constructor_WhenLocationExceedsMaximumLength_ThrowsArgumentException()
  {
    // Arrange
    var location =
        new string('L', 201);

    var start =
        DateTime.UtcNow.AddDays(2);

    // Act
    var action =
        () => new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            location);

    // Assert
    Assert.Throws<
        ArgumentException>(
        action);
  }

}