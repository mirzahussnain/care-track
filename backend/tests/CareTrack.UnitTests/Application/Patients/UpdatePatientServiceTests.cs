using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Patients.UpdatePatient;
using CareTrack.Domain.Entities;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
namespace CareTrack.UnitTests.Application.Patients;

public class UpdatePatientServiceTests
{
  private static UpdatePatientService CreateService(
  FakePatientRepository repository)
  {
    return new UpdatePatientService(
        repository,
        NullLogger<UpdatePatientService>.Instance);
  }

  private static Patient CreatePatient()
  {
    return new Patient(
        "PAT-UPD-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));
  }

  [Fact]
  public async Task ExecuteAsync_WithValidName_UpdatesName()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "Adam",
        "Jones",
        patient.DateOfBirth,
        rowVersion);

    // Act
    var result = await service.ExecuteAsync(command);

    // Assert
    Assert.Equal("Adam", result.FirstName);
    Assert.Equal("Jones", result.LastName);
  }

  [Fact]
  public async Task ExecuteAsync_WithValidDateOfBirth_UpdatesDateOfBirth()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var newDateOfBirth =
        new DateOnly(1992, 8, 15);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        patient.FirstName,
        patient.LastName,
        newDateOfBirth,
        rowVersion);

    // Act
    var result = await service.ExecuteAsync(command);

    // Assert
    Assert.Equal(
        newDateOfBirth,
        result.DateOfBirth);
  }

  [Fact]
  public async Task ExecuteAsync_WithUnknownId_ThrowsNotFoundException()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        Guid.NewGuid(),
        "John",
        "Smith",
        new DateOnly(1990, 5, 20),
        rowVersion);

    // Act + Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(command));

    // Since the patient was never found,
    // concurrency setup should never occur.
    Assert.Null(
        repository.LastOriginalRowVersion);
  }

  [Fact]
  public async Task ExecuteAsync_WithBlankFirstName_ThrowsArgumentException()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "   ",
        "Smith",
        patient.DateOfBirth,
        rowVersion);

    // Act + Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    // Validation should fail before the
    // repository concurrency token is configured.
    Assert.Null(
        repository.LastOriginalRowVersion);
  }

  [Fact]
  public async Task ExecuteAsync_WithBlankLastName_ThrowsArgumentException()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "John",
        "   ",
        patient.DateOfBirth,
        rowVersion);

    // Act + Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    Assert.Null(
        repository.LastOriginalRowVersion);
  }

  [Fact]
  public async Task ExecuteAsync_WithFutureDateOfBirth_ThrowsArgumentException()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var futureDate = DateOnly
        .FromDateTime(DateTime.UtcNow)
        .AddDays(1);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "Adam",
        "Jones",
        futureDate,
        rowVersion);

    // Act + Assert
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    Assert.Null(
        repository.LastOriginalRowVersion);
  }

  [Fact]
  public async Task ExecuteAsync_WithInvalidDateOfBirth_DoesNotPartiallyUpdatePatient()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var originalFirstName =
        patient.FirstName;

    var originalLastName =
        patient.LastName;

    var originalDateOfBirth =
        patient.DateOfBirth;

    var futureDate = DateOnly
        .FromDateTime(DateTime.UtcNow)
        .AddDays(1);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "Adam",
        "Jones",
        futureDate,
        rowVersion);

    // Act
    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(command));

    // Assert
    Assert.Equal(
        originalFirstName,
        patient.FirstName);

    Assert.Equal(
        originalLastName,
        patient.LastName);

    Assert.Equal(
        originalDateOfBirth,
        patient.DateOfBirth);

    // Since validation failed,
    // concurrency configuration should not occur.
    Assert.Null(
        repository.LastOriginalRowVersion);
  }

  [Fact]
  public async Task ExecuteAsync_WithValidUpdate_DoesNotChangeImmutableFields()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var originalId =
        patient.Id;

    var originalPatientReference =
        patient.PatientReference;

    var originalCreatedAt =
        patient.CreatedAt;

    var command = new UpdatePatientCommand(
        patient.Id,
        "Adam",
        "Jones",
        new DateOnly(1992, 8, 15),
        rowVersion);

    // Act
    var result = await service.ExecuteAsync(command);

    // Assert - editable values changed
    Assert.Equal(
        "Adam",
        result.FirstName);

    Assert.Equal(
        "Jones",
        result.LastName);

    Assert.Equal(
        new DateOnly(1992, 8, 15),
        result.DateOfBirth);

    // Assert - immutable values did not change
    Assert.Equal(
        originalId,
        result.Id);

    Assert.Equal(
        originalPatientReference,
        result.PatientReference);

    Assert.Equal(
        originalCreatedAt,
        result.CreatedAt);
  }

  [Fact]
  public async Task ExecuteAsync_WithValidUpdate_SetsOriginalRowVersion()
  {
    // Arrange
    var repository = new FakePatientRepository();
    var patient = CreatePatient();

    await repository.AddAsync(patient);

    var service = CreateService(repository);

    var rowVersion = new byte[]
    {
            1, 2, 3
    };

    var command = new UpdatePatientCommand(
        patient.Id,
        "Adam",
        "Jones",
        patient.DateOfBirth,
        rowVersion);

    // Act
    await service.ExecuteAsync(command);

    // Assert
    Assert.NotNull(
        repository.LastOriginalRowVersion);

    Assert.Equal(
        rowVersion,
        repository.LastOriginalRowVersion);
  }
}