using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.UnitTests.Fakes;
namespace CareTrack.UnitTests.Application;

public class CreatePatientServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WithValidCommand_CreatesPatient()
  {
    //Arrange
    var repository = new FakePatientRepository();
    var service = new CreatePatientService(repository);
    var command = new CreatePatientCommand(
        "PAT-002",
         "Jane",
          "Doe",
        new DateOnly(1995, 4, 10));

    //Act
    var patient = await service.ExecuteAsync(command);

    //Assert
    Assert.Equal("PAT-002", patient.PatientReference);
    Assert.Equal("Jane", patient.FirstName);
    Assert.Equal("Doe", patient.LastName);
    Assert.Equal(new DateOnly(1995, 4, 10), patient.DateOfBirth);
  }

  [Fact]
  public async Task ExecuteAsync_WithDuplicateReference_ThrowsExcetpion()
  {
    //Arrange
    var repository = new FakePatientRepository();
    var service = new CreatePatientService(repository);
    var firstCommand = new CreatePatientCommand(
      "PAT-002",
            "Jane",
            "Doe",
            new DateOnly(1995, 4, 10)
    );
    await service.ExecuteAsync(firstCommand);

    var duplicateCommand = new CreatePatientCommand(
            "PAT-002",
            "John",
            "Smith",
            new DateOnly(1990, 1, 1));

    //Act+Assert
    await Assert.ThrowsAsync<ConflictException>(() => service.ExecuteAsync(duplicateCommand));
  }
}