using CareTrack.Application.Appointments.CheckInAppointment;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using CareTrack.UnitTests.TestSupport.Helpers;
using Microsoft.Extensions.Logging.Abstractions;


namespace CareTrack.UnitTests.Application.Appointments;

public class CheckInAppointmentServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WithScheduledAppointment_ChecksInAppointment()
  {
    // Arrange
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

    var logger =
        NullLogger<CheckInAppointmentService>
            .Instance;

    var service =
        new CheckInAppointmentService(
            repository,
            logger);

    // Act
    var result =
        await service.ExecuteAsync(
            appointment.Id);

    // Assert
    Assert.Equal(
        AppointmentStatus.CheckedIn,
        result.Status);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var repository =
        new FakeAppointmentRepository();

    var logger =
        NullLogger<CheckInAppointmentService>
            .Instance;

    var service =
        new CheckInAppointmentService(
            repository,
            logger);

    // Act
    var action =
        () => service.ExecuteAsync(
            Guid.NewGuid());

    // Assert
    await Assert.ThrowsAsync<
        NotFoundException>(
        action);
  }
}