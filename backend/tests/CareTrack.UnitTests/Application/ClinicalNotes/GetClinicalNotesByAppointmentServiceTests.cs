using CareTrack.Application.ClinicalNotes.GetClinicalNotesByAppointment;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;

namespace CareTrack.UnitTests.Application.ClinicalNotes;

public class GetClinicalNotesByAppointmentServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenAppointmentExists_ReturnsAppointmentNotes()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var clinicalNoteRepository =
        new FakeClinicalNoteRepository();

    var start =
        DateTime.UtcNow.AddDays(3);

    var appointment =
        new Appointment(
            "APT-NOTES-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    await appointmentRepository.AddAsync(
        appointment);

    await clinicalNoteRepository.AddAsync(
        new ClinicalNote(
            appointment.Id,
            "First note",
            "clinician.demo"));

    await clinicalNoteRepository.AddAsync(
        new ClinicalNote(
            appointment.Id,
            "Second note",
            "clinician.demo"));

    var service =
        new GetClinicalNotesByAppointmentService(
            clinicalNoteRepository,
            appointmentRepository);

    // Act
    var result =
        await service.ExecuteAsync(
            appointment.Id);

    // Assert
    Assert.Equal(
        2,
        result.Count);

    Assert.All(
        result,
        note =>
            Assert.Equal(
                appointment.Id,
                note.AppointmentId));
  }


  [Fact]
  public async Task ExecuteAsync_WhenAppointmentHasNoNotes_ReturnsEmptyList()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var clinicalNoteRepository =
        new FakeClinicalNoteRepository();

    var start =
        DateTime.UtcNow.AddDays(3);

    var appointment =
        new Appointment(
            "APT-NOTES-002",
            Guid.NewGuid(),
            Guid.NewGuid(),
            AppointmentType.Consultation,
            start,
            start.AddMinutes(30),
            "Birmingham Clinic");

    await appointmentRepository.AddAsync(
        appointment);

    var service =
        new GetClinicalNotesByAppointmentService(
            clinicalNoteRepository,
            appointmentRepository);

    // Act
    var result =
        await service.ExecuteAsync(
            appointment.Id);

    // Assert
    Assert.Empty(
        result);
  }

  [Fact]
  public async Task ExecuteAsync_WhenAppointmentDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var appointmentRepository =
        new FakeAppointmentRepository();

    var clinicalNoteRepository =
        new FakeClinicalNoteRepository();

    var service =
        new GetClinicalNotesByAppointmentService(
            clinicalNoteRepository,
            appointmentRepository);

    // Act
    var action =
        () => service.ExecuteAsync(
            Guid.NewGuid());

    // Assert
    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }
}