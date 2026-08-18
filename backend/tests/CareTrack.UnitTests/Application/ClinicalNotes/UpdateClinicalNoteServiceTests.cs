using CareTrack.Application.ClinicalNotes.UpdateClinicalNote;
using CareTrack.Application.Common.Exceptions;
using CareTrack.Domain.Entities;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;

namespace CareTrack.UnitTests.Application.ClinicalNotes;

public class UpdateClinicalNoteServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WhenNoteExists_UpdatesContent()
  {
    // Arrange
    var repository =
        new FakeClinicalNoteRepository();

    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original content",
            "clinician.demo");

    await repository.AddAsync(
        note);

    var logger =
        NullLogger<UpdateClinicalNoteService>
            .Instance;

    var service =
        new UpdateClinicalNoteService(
            repository,
            logger);

    var command =
        new UpdateClinicalNoteCommand(
            note.Id,
            "Updated content");

    // Act
    var result =
        await service.ExecuteAsync(
            command);

    // Assert
    Assert.Equal(
        "Updated content",
        result.Content);

    Assert.Equal(
        "clinician.demo",
        result.CreatedBy);

    Assert.NotNull(
        result.UpdatedAt);
  }

  [Fact]
  public async Task ExecuteAsync_WhenNoteDoesNotExist_ThrowsNotFoundException()
  {
    var repository =
        new FakeClinicalNoteRepository();

    var logger =
        NullLogger<UpdateClinicalNoteService>
            .Instance;

    var service =
        new UpdateClinicalNoteService(
            repository,
            logger);

    var command =
        new UpdateClinicalNoteCommand(
            Guid.NewGuid(),
            "Updated");

    var action =
        () => service.ExecuteAsync(
            command);

    await Assert.ThrowsAsync<NotFoundException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenContentIsBlank_ThrowsArgumentException()
  {
    var repository =
        new FakeClinicalNoteRepository();

    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    await repository.AddAsync(
        note);

    var logger =
        NullLogger<UpdateClinicalNoteService>
            .Instance;

    var service =
        new UpdateClinicalNoteService(
            repository,
            logger);

    var command =
        new UpdateClinicalNoteCommand(
            note.Id,
            "   ");

    var action =
        () => service.ExecuteAsync(
            command);

    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }

  [Fact]
  public async Task ExecuteAsync_WhenContentExceedsMaximum_ThrowsArgumentException()
  {
    var repository =
        new FakeClinicalNoteRepository();

    var note =
        new ClinicalNote(
            Guid.NewGuid(),
            "Original",
            "clinician.demo");

    await repository.AddAsync(
        note);

    var logger =
        NullLogger<UpdateClinicalNoteService>
            .Instance;

    var service =
        new UpdateClinicalNoteService(
            repository,
            logger);

    var command =
        new UpdateClinicalNoteCommand(
            note.Id,
            new string('a', 5001));

    var action =
        () => service.ExecuteAsync(
            command);

    await Assert.ThrowsAsync<ArgumentException>(
        action);
  }
}