using CareTrack.Application.Appointments.CreateAppointment;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using CareTrack.UnitTests.Helpers;
using Microsoft.Extensions.Logging.Abstractions;


namespace CareTrack.UnitTests.Application;

public class CreateAppointmentTests
{
  [Fact]
  public async Task ExecuteAsync_WithValidData_CreatesAppointment()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    var patient =
        new Patient(
            "PAT-001",
            "John",
            "Smith",
            new DateOnly(1990, 5, 20));

    await patientRepository.AddAsync(
        patient);

    var referral =
        new Referral(
            "REF-001",
            patient.Id,
            ReferralPriority.Routine,
            "Persistent shoulder pain.");

    await referralRepository.AddAsync(
        referral);

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            logger);

    var start =
        DateTime.UtcNow.AddDays(2);

    var command =
        new CreateAppointmentCommand(
            "APT-001",
            patient.Id,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Act
    var result =
        await service.ExecuteAsync(
            command);

    // Assert
    Assert.Equal(
        "APT-001",
        result.AppointmentReference);

    Assert.Equal(
        patient.Id,
        result.PatientId);

    Assert.Equal(
        referral.Id,
        result.ReferralId);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        result.Status);

    Assert.Single(
        appointmentRepository.Appointments);
  }

  [Fact]
  public async Task ExecuteAsync_WhenPatientDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            logger);

    var start =
        DateTime.UtcNow.AddDays(2);

    var command =
        new CreateAppointmentCommand(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Act
    var action =
        () => service.ExecuteAsync(
            command);

    // Assert
    await Assert.ThrowsAsync<
        NotFoundException>(
        action);

    Assert.Empty(
        appointmentRepository.Appointments);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient =
        new Patient(
            "PAT-001",
            "John",
            "Smith",
            new DateOnly(1990, 5, 20));

    await patientRepository.AddAsync(
        patient);

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            logger);

    var start =
        DateTime.UtcNow.AddDays(2);

    var command =
        new CreateAppointmentCommand(
            "APT-001",
            patient.Id,
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Act
    var action =
        () => service.ExecuteAsync(
            command);

    // Assert
    await Assert.ThrowsAsync<
        NotFoundException>(
        action);

    Assert.Empty(
        appointmentRepository.Appointments);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralBelongsToDifferentPatient_ThrowsArgumentException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patientOne =
        new Patient(
            "PAT-001",
            "John",
            "Smith",
            new DateOnly(1990, 5, 20));

    var patientTwo =
        new Patient(
            "PAT-002",
            "Jane",
            "Smith",
            new DateOnly(1992, 7, 10));

    await patientRepository.AddAsync(
        patientOne);

    await patientRepository.AddAsync(
        patientTwo);

    var referral =
        new Referral(
            "REF-001",
            patientOne.Id,
            ReferralPriority.Routine,
            "Persistent shoulder pain.");

    await referralRepository.AddAsync(
        referral);

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            logger);

    var start =
        DateTime.UtcNow.AddDays(2);

    var command =
        new CreateAppointmentCommand(
            "APT-001",

            // deliberately wrong patient:
            patientTwo.Id,

            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Act
    var action =
        () => service.ExecuteAsync(
            command);

    // Assert
    await Assert.ThrowsAsync<
        ArgumentException>(
        action);

    Assert.Empty(
        appointmentRepository.Appointments);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentReferenceAlreadyExists_ThrowsConflictException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient =
        new Patient(
            "PAT-001",
            "John",
            "Smith",
            new DateOnly(1990, 5, 20));

    await patientRepository.AddAsync(
        patient);

    var referral =
        new Referral(
            "REF-001",
            patient.Id,
            ReferralPriority.Routine,
            "Persistent shoulder pain.");

    await referralRepository.AddAsync(
        referral);

    var existingStart =
        DateTime.UtcNow.AddDays(1);

    var existingAppointment =
        new Appointment(
            "APT-001",
            patient.Id,
            referral.Id,
            AppointmentType.Consultation,
            existingStart,
            existingStart.AddMinutes(30),
            "Birmingham Clinic");

    await appointmentRepository.AddAsync(
        existingAppointment);

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            logger);

    var start =
        DateTime.UtcNow.AddDays(2);

    var command =
        new CreateAppointmentCommand(
            "APT-001",
            patient.Id,
            referral.Id,
            AppointmentType.FollowUp,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Act
    var action =
        () => service.ExecuteAsync(
            command);

    // Assert
    await Assert.ThrowsAsync<
        ConflictException>(
        action);
  }
}