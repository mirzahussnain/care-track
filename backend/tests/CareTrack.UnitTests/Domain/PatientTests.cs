using CareTrack.Domain.Entities;
namespace CareTrack.UnitTests.Domain;

public class PatientTests
{
  [Fact]
  public void Constructor_WithValidData_CreatesPatient()
  {
    var patient = new Patient("PAT-001",
                "John",
                "Smith",
                new DateOnly(1990, 5, 20));
    Assert.Equal("PAT-001", patient.PatientReference);
    Assert.Equal("John", patient.FirstName);
    Assert.Equal("Smith", patient.LastName);
    Assert.Equal(new DateOnly(1990, 5, 20), patient.DateOfBirth);
  }

  [Fact]
  public void Constructor_WithEmptyFirstName_ThrowsArgumentException()
  {
    var exception = Assert.Throws<ArgumentException>(() => new Patient("PAT-001",
          "",
          "Smith",
          new DateOnly(1990, 5, 20)));
    Assert.Equal("firstName", exception.ParamName);
  }

  [Fact]
  public void Constructor_WithFutureDateOfBirth_ThrowsArgumentException()
  {
    var futureDate =
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    Assert.Throws<ArgumentException>(() =>
        new Patient(
            "PAT-001",
            "John",
            "Smith",
            futureDate));
  }

  [Fact]
  public void FullName_Returns_CombinedFirstAndLastName()
  {
    var patient = new Patient(
            "PAT-001",
            "John",
            "Smith",
            new DateOnly(1990, 5, 20));

    Assert.Equal("John Smith", patient.FullName);
  }

  [Fact]
  public void Constructor_WithWhitespace_TrimsNamesAndReference()
  {
    var patient = new Patient(
        "  PAT-001  ",
        "  John  ",
        "  Smith  ",
        new DateOnly(1990, 5, 20));

    Assert.Equal("PAT-001", patient.PatientReference);
    Assert.Equal("John", patient.FirstName);
    Assert.Equal("Smith", patient.LastName);
  }

  [Fact]
  public void UpdateName_WithValidName_UpdatesPatient()
  {
    var patient = new Patient(
        "PAT-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));

    patient.UpdateName("Jane", "Doe");

    Assert.Equal("Jane", patient.FirstName);
    Assert.Equal("Doe", patient.LastName);
    Assert.Equal("Jane Doe", patient.FullName);
  }

  [Fact]
  public void UpdateName_WithEmptyFirstName_ThrowsArgumentException()
  {
    var patient = new Patient(
        "PAT-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));

    Assert.Throws<ArgumentException>(() =>
        patient.UpdateName("", "Doe"));
    Assert.Equal("John", patient.FirstName);
    Assert.Equal("Smith", patient.LastName);
  }

  [Fact]
  public void UpdateDateOfBirth_WithValidDate_UpdatesDateOfBirth()
  {
    var patient = new Patient(
    "PAT-001",
    "John",
    "Smith",
    new DateOnly(1990, 5, 20));


    var newDateOfBirth = new DateOnly(1991, 6, 15);
    patient.UpdateDateOfBirth(newDateOfBirth);

    Assert.Equal(newDateOfBirth, patient.DateOfBirth);

  }

  [Fact]
  public void UpdateDateOfBirth_WithFutureDate_ThrowsArgumentException()
  {
    var patient = new Patient(
        "PAT-001",
        "John",
        "Smith",
        new DateOnly(1990, 5, 20));

    var futureDate =
        DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

    Assert.Throws<ArgumentException>(() =>
        patient.UpdateDateOfBirth(futureDate));
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("     ")]
  public void Constructor_WithInvalidFirstName_ThrowsArgumentException(
    string firstName)
  {
    Assert.Throws<ArgumentException>(() =>
        new Patient(
            "PAT-001",
            firstName,
            "Smith",
            new DateOnly(1990, 5, 20)));
  }

}