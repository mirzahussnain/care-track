using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Patients.GetPatient;
using CareTrack.Domain.Entities;
using CareTrack.UnitTests.TestSupport.Fakes;
namespace CareTrack.UnitTests.Application.Patients;

public class GetPatientServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WithExistingPatient_ReturnsPatient()
  {
    // Arrange

    var repository = new FakePatientRepository();

    var patient = new Patient(
        "PAT-GET-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));

    await repository.AddAsync(patient);

    var service = new GetPatientService(repository);

    // Act

    var result = await service.ExecuteAsync(patient.Id);

    // Assert

    Assert.Equal(patient.Id, result.Id);
    Assert.Equal("PAT-GET-001", result.PatientReference);
  }

  [Fact]
  public async Task ExecuteAsync_WithUnknownId_ThrowsNotFoundException()
  {
    var repository = new FakePatientRepository();

    var service = new GetPatientService(repository);

    var unknownId = Guid.NewGuid();

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(unknownId));
  }
}