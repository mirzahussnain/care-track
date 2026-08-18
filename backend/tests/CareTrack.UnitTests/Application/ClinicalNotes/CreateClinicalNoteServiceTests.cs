using CareTrack.Application.ClinicalNotes.CreateClinicalNote;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.ClinicalNotes;

public class CreateClinicalNoteServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WithExistingAppointment_CreatesClinicalNote()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var clinicalNoteRepository =
        new FakeClinicalNoteRepository();

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

    await appointmentRepository
        .AddAsync(
            appointment);

    var logger =
        NullLogger<CreateClinicalNoteService>
            .Instance;

    var service =
        new CreateClinicalNoteService(
            clinicalNoteRepository,
            appointmentRepository,
            logger);

    var command =
        new CreateClinicalNoteCommand(
            appointment.Id,
            "Patient reports improvement.",
            "clinician.demo");

    // Act
    var result =
        await service.ExecuteAsync(
            command);

    // Assert
    Assert.NotEqual(
        Guid.Empty,
        result.Id);

    Assert.Equal(
        appointment.Id,
        result.AppointmentId);

    Assert.Equal(
        "Patient reports improvement.",
        result.Content);

    Assert.Single(
        clinicalNoteRepository.Notes);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var clinicalNoteRepository =
        new FakeClinicalNoteRepository();

    var logger =
        NullLogger<CreateClinicalNoteService>
            .Instance;

    var service =
        new CreateClinicalNoteService(
            clinicalNoteRepository,
            appointmentRepository,
            logger);

    var command =
        new CreateClinicalNoteCommand(
            Guid.NewGuid(),
            "Clinical note",
            "clinician.demo");

    // Act
    var action =
        () => service.ExecuteAsync(
            command);

    // Assert
    await Assert.ThrowsAsync<
        NotFoundException>(
        action);

    Assert.Empty(
        clinicalNoteRepository.Notes);
  }

}