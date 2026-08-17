

using CareTrack.Application.Appointments.CreateAppointment;
using CareTrack.Application.Appointments.GetAppointmentById;
using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application;

public class SearchAppointmentsTests
{
  [Fact]
  public async Task ExecuteAsync_WhenPageIsLessThanOne_ThrowsArgumentException()
  {
    // Arrange
    var repository =
        new FakeAppointmentRepository();

    var service =
        new SearchAppointmentsService(
            repository);

    var query =
        new AppointmentSearchCommand(
            PatientId: null,
            ReferralId: null,
            Status: null,
            AppointmentType: null,
            Location: null,
            ScheduledFrom: null,
            ScheduledTo: null,
            Page: 0,
            PageSize: 20,
            SortBy: "scheduledStart",
            SortDirection: "asc");

    // Act
    var action =
        () => service.ExecuteAsync(query);

    // Assert
    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenPageSizeExceedsMaximum_ThrowsArgumentException()
  {
    var repository =
        new FakeAppointmentRepository();

    var service =
        new SearchAppointmentsService(
            repository);

    var query =
        new AppointmentSearchCommand(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1,
            101,
            "scheduledStart",
            "asc");

    var action =
        () => service.ExecuteAsync(query);

    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenScheduledToIsBeforeScheduledFrom_ThrowsArgumentException()
  {
    var repository =
        new FakeAppointmentRepository();

    var service =
        new SearchAppointmentsService(
            repository);

    var from =
        DateTime.UtcNow.AddDays(1);

    var query =
        new AppointmentSearchCommand(
            null,
            null,
            null,
            null,
            null,
            from,
            from.AddHours(-1),
            1,
            20,
            "scheduledStart",
            "asc");

    var action =
        () => service.ExecuteAsync(query);

    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }


  [Fact]
  public async Task ExecuteAsync_WhenSortFieldIsInvalid_ThrowsArgumentException()
  {
    var repository =
        new FakeAppointmentRepository();

    var service =
        new SearchAppointmentsService(
            repository);

    var query =
        new AppointmentSearchCommand(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            1,
            20,
            "banana",
            "asc");

    var action =
        () => service.ExecuteAsync(query);

    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
  {
    var repository =
        new FakeAppointmentRepository();

    var service =
        new GetAppointmentByIdService(
            repository);

    var action =
        () => service.ExecuteAsync(
            Guid.NewGuid());

    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentExists_ReturnsAppointment()
  {
    var repository =
        new FakeAppointmentRepository();

    var start =
        DateTime.UtcNow.AddDays(2);

    var appointment =
        new Appointment(
            "APT-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    await repository.AddAsync(
        appointment);

    var service =
        new GetAppointmentByIdService(
            repository);

    var result =
        await service.ExecuteAsync(
            appointment.Id);

    Assert.Equal(
        appointment.Id,
        result.Id);

    Assert.Equal(
        "APT-001",
        result.AppointmentReference);
  }

  [Fact]
  public async Task ExecuteAsync_WhenPatientHasOverlappingAppointment_ThrowsConflictException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository
        {
          SchedulingConflictExists = true
        };
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
    var action =
        () => service.ExecuteAsync(command);

    // Assert
    await Assert.ThrowsAsync<ConflictException>(
        action);
  }

}