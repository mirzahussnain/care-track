using CareTrack.Application.ClinicalNotes.GetClinicalNoteById;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.UnitTests.Fakes;

namespace CareTrack.UnitTests.Application;

public class GetClinicalNoteByIdServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenNoteExists_ReturnsClinicalNote()
  {
    // Arrange
    var repository =
        new FakeClinicalNoteRepository();

    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Patient improving.",
            "clinician.demo");

    await repository.AddAsync(
        note);

    var service =
        new GetClinicalNoteByIdService(
            repository);

    // Act
    var result =
        await service.ExecuteAsync(
            note.Id);

    // Assert
    Assert.Equal(
        note.Id,
        result.Id);

    Assert.Equal(
        note.AppointmentId,
        result.AppointmentId);

    Assert.Equal(
        note.Content,
        result.Content);

    Assert.Equal(
        note.CreatedBy,
        result.CreatedBy);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoteDoesNotExist_ThrowsNotFoundException()
  {
    // Arrange
    var repository =
        new FakeClinicalNoteRepository();

    var service =
        new GetClinicalNoteByIdService(
            repository);

    // Act
    var action =
        () => service.ExecuteAsync(
            Guid.NewGuid());

    // Assert
    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }
}