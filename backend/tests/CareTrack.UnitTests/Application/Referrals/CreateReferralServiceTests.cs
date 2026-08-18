using CareTrack.Application.Common.Exceptions;
using CareTrack.Application.Referrals.CreateReferral;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.UnitTests.TestSupport.Fakes;
using Microsoft.Extensions.Logging.Abstractions;
namespace CareTrack.UnitTests.Application.Referrals;

public class CreateReferralServiceTests()
{
  private static Patient CreatePatient()
  {
    return new Patient(
        "PAT-REF-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));
  }
  private static CreateReferralService CreateService(
       FakePatientRepository patientRepository,
       FakeReferralRepository referralRepository)
  {
    return new CreateReferralService(
        patientRepository,
        referralRepository,
        NullLogger<CreateReferralService>.Instance);
  }
  [Fact]
  public async Task ExecuteAsync_WithValidCommand_CreatesReferral()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient = CreatePatient();

    await patientRepository.AddAsync(
        patient);

    var service = CreateService(patientRepository, referralRepository);

    var command =
        new CreateReferralCommand(
            "REF-001",
            patient.Id,
            ReferralPriority.Routine,
            "Persistent shoulder pain.");

    // Act
    var referral =
        await service.ExecuteAsync(
            command);

    // Assert
    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    Assert.Equal(
        patient.Id,
        referral.PatientId);

    Assert.Single(
        referralRepository.Referrals);
  }

  [Fact]
  public async Task ExecuteAsync_WithValidCommand_AddsReferralToRepository()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient = CreatePatient();

    await patientRepository.AddAsync(patient);

    var service =
        CreateService(
            patientRepository,
            referralRepository);

    var command =
        new CreateReferralCommand(
            "REF-002",
            patient.Id,
            ReferralPriority.Urgent,
            "Urgent specialist review required.");

    // Act
    var referral =
        await service.ExecuteAsync(command);

    // Assert
    var storedReferral =
        Assert.Single(
            referralRepository.Referrals);

    Assert.Equal(
        referral.Id,
        storedReferral.Id);

    Assert.Equal(
        "REF-002",
        storedReferral.ReferralReference);
  }

  [Fact]
  public async Task ExecuteAsync_WithUnknownPatient_ThrowsNotFoundException()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var service =
        CreateService(
            patientRepository,
            referralRepository);

    var command =
        new CreateReferralCommand(
            "REF-003",
            Guid.NewGuid(),
            ReferralPriority.Routine,
            "Routine specialist review.");

    // Act + Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => service.ExecuteAsync(command));

    Assert.Empty(
        referralRepository.Referrals);
  }

  [Fact]
  public async Task ExecuteAsync_WithDuplicateReference_ThrowsConflictException()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient = CreatePatient();

    await patientRepository.AddAsync(patient);

    var existingReferral =
        new Referral(
            "REF-004",
            patient.Id,
            ReferralPriority.Routine,
            "Existing referral.");

    await referralRepository.AddAsync(
        existingReferral);

    var service =
        CreateService(
            patientRepository,
            referralRepository);

    var command =
        new CreateReferralCommand(
            "REF-004",
            patient.Id,
            ReferralPriority.Urgent,
            "Attempted duplicate referral.");

    // Act + Assert
    await Assert.ThrowsAsync<ConflictException>(
        () => service.ExecuteAsync(command));

    Assert.Single(
        referralRepository.Referrals);
  }

  [Fact]
  public async Task ExecuteAsync_WithValidCommand_CreatedReferralStartsAsDraft()
  {
    // Arrange
    var patientRepository =
        new FakePatientRepository();

    var referralRepository =
        new FakeReferralRepository();

    var patient = CreatePatient();

    await patientRepository.AddAsync(patient);

    var service =
        CreateService(
            patientRepository,
            referralRepository);

    var command =
        new CreateReferralCommand(
            "REF-005",
            patient.Id,
            ReferralPriority.Routine,
            "Routine assessment.");

    // Act
    var referral =
        await service.ExecuteAsync(command);

    // Assert
    Assert.Equal(
        ReferralStatus.Draft,
        referral.Status);

    Assert.Null(
        referral.SubmittedAt);
  }
}