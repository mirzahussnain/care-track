using System.Data;
using CareTrack.Application.Appointments.CreateAppointment;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;


namespace CareTrack.UnitTests.Application.Appointments;

public class CreateAppointmentServiceTests
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

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign("Integration Test Team");

    await referralRepository.AddAsync(
        referral);

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    var transaction = new FakeApplicationTransaction();
    var service = new CreateAppointmentService(
     appointmentRepository,
     patientRepository,
     referralRepository,
     transaction,
     logger
    );

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
    ReferralStatus.Scheduled,
    referral.Status);
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

    Assert.Equal(
        IsolationLevel.Serializable,
        transaction.RequestedIsolationLevel);
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

    var transaction = new FakeApplicationTransaction();
    var service = new CreateAppointmentService(
    appointmentRepository,
     patientRepository,
     referralRepository,
     transaction,
     logger
    );

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

    var transaction = new FakeApplicationTransaction();
    var service = new CreateAppointmentService(
     appointmentRepository,
     patientRepository,
     referralRepository,
     transaction,
     logger
    );

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

    var transaction = new FakeApplicationTransaction();
    var service = new CreateAppointmentService(
     appointmentRepository,
     patientRepository,
     referralRepository,
     transaction,
     logger
    );

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

    var transaction = new FakeApplicationTransaction();
    var service = new CreateAppointmentService(
     appointmentRepository,
     patientRepository,
     referralRepository,
     transaction,
     logger
    );

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

  [Fact]
  public async Task ExecuteAsync_WhenReferralIsDraft_ThrowsConflictException()
  {
    // Arrange

    var patientId =
        Guid.NewGuid();

    var patient =
        new Patient(
            "PAT-4G-001",
            "Test",
            "Patient",
            new DateOnly(1995, 1, 1));

    var referral =
        new Referral(
            "REF-4G-001",
            patient.Id,
            ReferralPriority.Routine,
            "Test referral");

    // Important:
    // Do NOT submit / triage / accept / assign.
    // Referral intentionally remains Draft.

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    var transaction =
        new FakeApplicationTransaction();

    var logger =
        NullLogger<CreateAppointmentService>
            .Instance;

    await patientRepository.AddAsync(
        patient);

    await referralRepository.AddAsync(
        referral);

    var start =
        DateTime.UtcNow.AddDays(3);

    var command =
        new CreateAppointmentCommand(
            "APT-4G-001",
            patient.Id,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Cardiology Clinic");

    var service =
        new CreateAppointmentService(
            appointmentRepository,
            patientRepository,
            referralRepository,
            transaction,
            logger);

    // Act

    var action =
        () => service.ExecuteAsync(
            command);

    // Assert

    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);
  }
}