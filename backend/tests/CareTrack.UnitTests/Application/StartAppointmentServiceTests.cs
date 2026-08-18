using CareTrack.Application.Appointments.StartAppointment;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.Appointments;

public class StartAppointmentServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var referralRepository =
        new FakeReferralRepository();

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    var appointmentId =
        Guid.NewGuid();

    // Act
    var action =
        () => service.ExecuteAsync(
            appointmentId);

    // Assert
    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referralId =
        Guid.NewGuid();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-START-001",
            patientId,
            referralId,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    appointment.CheckIn();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    var action =
        () => service.ExecuteAsync(
            appointment.Id);

    // Assert
    await Assert.ThrowsAsync<NotFoundException>(
        action);

    Assert.Equal(
        AppointmentStatus.CheckedIn,
        appointment.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentIsNotCheckedIn_ThrowsInvalidStateTransitionException()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referral =
        new Referral(
            "REF-START-001",
            patientId,
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");

    referral.Schedule();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-START-002",
            patientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    // Appointment intentionally remains Scheduled.

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    await referralRepository.AddAsync(
        referral);

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    var action =
        () => service.ExecuteAsync(
            appointment.Id);

    // Assert
    await Assert.ThrowsAsync<
        InvalidStateTransitionException>(
        action);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        appointment.Status);

    Assert.Equal(
        ReferralStatus.Scheduled,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentIsCheckedIn_StartsAppointment()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referral =
        new Referral(
            "REF-START-002",
            patientId,
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");

    referral.Schedule();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-START-003",
            patientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    appointment.CheckIn();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    await referralRepository.AddAsync(
        referral);

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    var result =
        await service.ExecuteAsync(
            appointment.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.InProgress,
        appointment.Status);

    Assert.NotNull(
        appointment.StartedAt);

    Assert.NotNull(
        appointment.UpdatedAt);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralIsScheduled_MovesReferralToInProgress()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referral =
        new Referral(
            "REF-START-003",
            patientId,
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");

    referral.Schedule();

    Assert.Equal(
        ReferralStatus.Scheduled,
        referral.Status);

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-START-004",
            patientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    appointment.CheckIn();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    await referralRepository.AddAsync(
        referral);

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    await service.ExecuteAsync(
        appointment.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.InProgress,
        appointment.Status);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);

    Assert.NotNull(
        referral.UpdatedAt);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralMovesToInProgress_AddsReferralHistoryEntry()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referral =
        new Referral(
            "REF-START-004",
            patientId,
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");

    referral.Schedule();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-START-005",
            patientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    appointment.CheckIn();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    await referralRepository.AddAsync(
        referral);

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    await service.ExecuteAsync(
        appointment.Id);

    // Assert
    var historyEntry =
        Assert.Single(
            referral.History,
            history =>
                history.EventType ==
                ReferralHistoryEventType.Started);

    Assert.Equal(
        ReferralStatus.Scheduled,
        historyEntry.FromStatus);

    Assert.Equal(
        ReferralStatus.InProgress,
        historyEntry.ToStatus);

    Assert.Equal(
        referral.UpdatedAt,
        historyEntry.OccurredAt);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralIsAlreadyInProgress_DoesNotStartReferralAgain()
  {
    // Arrange
    var patientId =
        Guid.NewGuid();

    var referral =
        new Referral(
            "REF-START-005",
            patientId,
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");

    referral.Schedule();
    referral.StartProgress();

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);

    var startedHistoryCountBefore =
        referral.History.Count(
            history =>
                history.EventType ==
                ReferralHistoryEventType.Started);

    var start =
        DateTime.UtcNow.AddDays(2);

    var appointment =
        new Appointment(
            "APT-START-006",
            patientId,
            referral.Id,
            AppointmentType.FollowUp,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    appointment.CheckIn();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await appointmentRepository.AddAsync(
        appointment);

    var referralRepository =
        new FakeReferralRepository();

    await referralRepository.AddAsync(
        referral);

    var transaction =
        new FakeApplicationTransaction();

    var service =
        new StartAppointmentService(
            appointmentRepository,
            referralRepository,
            transaction,
            NullLogger<StartAppointmentService>.Instance);

    // Act
    await service.ExecuteAsync(
        appointment.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.InProgress,
        appointment.Status);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);

    var startedHistoryCountAfter =
        referral.History.Count(
            history =>
                history.EventType ==
                ReferralHistoryEventType.Started);

    Assert.Equal(
        startedHistoryCountBefore,
        startedHistoryCountAfter);
  }
}