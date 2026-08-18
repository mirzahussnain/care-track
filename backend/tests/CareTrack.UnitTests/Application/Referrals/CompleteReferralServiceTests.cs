using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.CompleteReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
namespace CareTrack.UnitTests.Application.Referrals;

public class CompleteReferralServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenReferralDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    var logger =
        NullLogger<CompleteReferralService>
            .Instance;

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            logger);

    var command =
        new CompleteReferralCommand(
            Guid.NewGuid());

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralHasNoAppointments_ThrowsConflictException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-001",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    var logger =
        NullLogger<CompleteReferralService>
            .Instance;

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            logger);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoAppointmentIsCompleted_ThrowsConflictException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-002",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-COMPLETE-001",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Cardiology Clinic");

    Assert.Equal(
        AppointmentStatus.Scheduled,
        appointment.Status);

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        appointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);
  }
  [Fact]
  public async Task ExecuteAsync_WhenScheduledAppointmentRemains_ThrowsConflictException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-003",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var firstStart =
        DateTime.UtcNow.AddDays(1);

    var completedAppointment =
        new Appointment(
            "APT-COMPLETE-002",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            firstStart,
            firstStart.AddMinutes(30),
            "Cardiology Clinic");

    completedAppointment.CheckIn();
    completedAppointment.Start();
    completedAppointment.Complete();

    var secondStart =
        firstStart.AddHours(2);

    var scheduledAppointment =
        new Appointment(
            "APT-COMPLETE-003",
            referral.PatientId,
            referral.Id,
            AppointmentType.FollowUp,
            secondStart,
            secondStart.AddMinutes(30),
            "Cardiology Clinic");

    Assert.Equal(
        AppointmentStatus.Completed,
        completedAppointment.Status);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        scheduledAppointment.Status);

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        completedAppointment);

    await appointmentRepository.AddAsync(
        scheduledAppointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenCheckedInAppointmentRemains_ThrowsConflictException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-004",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var firstStart =
        DateTime.UtcNow.AddDays(1);

    var completedAppointment =
        new Appointment(
            "APT-COMPLETE-004",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            firstStart,
            firstStart.AddMinutes(30),
            "Cardiology Clinic");

    completedAppointment.CheckIn();
    completedAppointment.Start();
    completedAppointment.Complete();

    var secondStart =
        firstStart.AddHours(2);

    var checkedInAppointment =
        new Appointment(
            "APT-COMPLETE-005",
            referral.PatientId,
            referral.Id,
            AppointmentType.FollowUp,
            secondStart,
            secondStart.AddMinutes(30),
            "Cardiology Clinic");

    checkedInAppointment.CheckIn();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        completedAppointment);

    await appointmentRepository.AddAsync(
        checkedInAppointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenInProgressAppointmentRemains_ThrowsConflictException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-005",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var firstStart =
        DateTime.UtcNow.AddDays(1);

    var completedAppointment =
        new Appointment(
            "APT-COMPLETE-006",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            firstStart,
            firstStart.AddMinutes(30),
            "Cardiology Clinic");

    completedAppointment.CheckIn();
    completedAppointment.Start();
    completedAppointment.Complete();

    var secondStart =
        firstStart.AddHours(2);

    var inProgressAppointment =
        new Appointment(
            "APT-COMPLETE-007",
            referral.PatientId,
            referral.Id,
            AppointmentType.FollowUp,
            secondStart,
            secondStart.AddMinutes(30),
            "Cardiology Clinic");

    inProgressAppointment.CheckIn();
    inProgressAppointment.Start();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        completedAppointment);

    await appointmentRepository.AddAsync(
        inProgressAppointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);

    Assert.Equal(
        ReferralStatus.InProgress,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenCompletedAppointmentExists_CompletesReferral()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-006",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var start =
        DateTime.UtcNow.AddDays(1);

    var appointment =
        new Appointment(
            "APT-COMPLETE-008",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Cardiology Clinic");

    appointment.CheckIn();
    appointment.Start();
    appointment.Complete();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        appointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    await service.ExecuteAsync(
        command);

    // Assert
    Assert.Equal(
        ReferralStatus.Completed,
        referral.Status);

    Assert.NotNull(
        referral.UpdatedAt);

    var completedHistoryEntry =
Assert.Single(
    referral.History,
    history =>
        history.EventType ==
        ReferralHistoryEventType.Completed);

    Assert.Equal(
        ReferralStatus.InProgress,
        completedHistoryEntry.FromStatus);

    Assert.Equal(
        ReferralStatus.Completed,
        completedHistoryEntry.ToStatus);

    Assert.Equal(
        referral.UpdatedAt,
        completedHistoryEntry.OccurredAt);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnlyCompletedAndCancelledAppointmentsExist_CompletesReferral()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-007",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var firstStart =
        DateTime.UtcNow.AddDays(1);

    var completedAppointment =
        new Appointment(
            "APT-COMPLETE-009",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            firstStart,
            firstStart.AddMinutes(30),
            "Cardiology Clinic");

    completedAppointment.CheckIn();
    completedAppointment.Start();
    completedAppointment.Complete();

    var secondStart =
        firstStart.AddHours(2);

    var cancelledAppointment =
        new Appointment(
            "APT-COMPLETE-010",
            referral.PatientId,
            referral.Id,
            AppointmentType.FollowUp,
            secondStart,
            secondStart.AddMinutes(30),
            "Cardiology Clinic");

    cancelledAppointment.Cancel();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        completedAppointment);

    await appointmentRepository.AddAsync(
        cancelledAppointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    // Act
    await service.ExecuteAsync(
        new CompleteReferralCommand(
            referral.Id));

    // Assert
    Assert.Equal(
        ReferralStatus.Completed,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenOnlyCompletedAndDidNotAttendAppointmentsExist_CompletesReferral()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-008",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Cardiology review");

    referral.Submit();
    referral.StartTriage();
    referral.Accept();
    referral.Assign(
        "Cardiology Team");
    referral.Schedule();
    referral.StartProgress();

    var firstStart =
        DateTime.UtcNow.AddDays(1);

    var completedAppointment =
        new Appointment(
            "APT-COMPLETE-011",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            firstStart,
            firstStart.AddMinutes(30),
            "Cardiology Clinic");

    completedAppointment.CheckIn();
    completedAppointment.Start();
    completedAppointment.Complete();

    var secondStart =
        firstStart.AddHours(2);

    var didNotAttendAppointment =
        new Appointment(
            "APT-COMPLETE-012",
            referral.PatientId,
            referral.Id,
            AppointmentType.FollowUp,
            secondStart,
            secondStart.AddMinutes(30),
            "Cardiology Clinic");

    didNotAttendAppointment.MarkDidNotAttend();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        completedAppointment);

    await appointmentRepository.AddAsync(
        didNotAttendAppointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    // Act
    await service.ExecuteAsync(
        new CompleteReferralCommand(
            referral.Id));

    // Assert
    Assert.Equal(
        ReferralStatus.Completed,
        referral.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenReferralIsNotInProgress_ThrowsInvalidStateTransitionException()
  {
    // Arrange
    var referral =
        new Referral(
            "REF-COMPLETE-009",
            Guid.NewGuid(),
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
            "APT-COMPLETE-013",
            referral.PatientId,
            referral.Id,
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Cardiology Clinic");

    appointment.CheckIn();
    appointment.Start();
    appointment.Complete();

    var referralRepository =
        new FakeReferralRepository();

    var appointmentRepository =
        new FakeAppointmentRepository();

    await referralRepository.AddAsync(
        referral);

    await appointmentRepository.AddAsync(
        appointment);

    var service =
        new CompleteReferralService(
            referralRepository,
            appointmentRepository,
            NullLogger<CompleteReferralService>.Instance);

    var command =
        new CompleteReferralCommand(
            referral.Id);

    // Act
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<
        InvalidStateTransitionException>(
        action);

    Assert.Equal(
        ReferralStatus.Scheduled,
        referral.Status);
  }
}