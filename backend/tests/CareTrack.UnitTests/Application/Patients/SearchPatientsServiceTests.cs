using CareTrack.Application.Patients.SearchPatients;
using CareTrack.Domain.Entities;
using CareTrack.UnitTests.TestSupport.Fakes;

namespace CareTrack.UnitTests.Application.Patients;

public class SearchPatientsServiceTests
{
  [Fact]
  public async Task ExecuteAsync_WithoutSearch_ReturnsPaginatedPatients()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: null,
            Page: 1,
            PageSize: 2));

    Assert.Equal(2, result.Items.Count);
    Assert.Equal(4, result.TotalCount);
  }

  [Fact]
  public async Task ExecuteAsync_SearchesByFirstName()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: "Alice"));

    Assert.Single(result.Items);
    Assert.Equal("Alice", result.Items[0].FirstName);
  }

  [Fact]
  public async Task ExecuteAsync_SearchesByLastName()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: "Smith"));

    Assert.Equal(2, result.Items.Count);
    Assert.All(
        result.Items,
        patient => Assert.Equal("Smith", patient.LastName));
  }

  [Fact]
  public async Task ExecuteAsync_SearchesByPatientReference()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: "PAT-004"));

    Assert.Single(result.Items);
    Assert.Equal("PAT-004", result.Items[0].PatientReference);
  }

  [Fact]
  public async Task ExecuteAsync_PageTwo_ReturnsCorrectRecords()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: null,
            Page: 2,
            PageSize: 2));

    Assert.Equal(
        ["PAT-003", "PAT-004"],
        result.Items.Select(
            patient => patient.PatientReference));
  }

  [Fact]
  public async Task ExecuteAsync_RespectsPageSize()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            PageSize: 3));

    Assert.Equal(3, result.Items.Count);
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public async Task ExecuteAsync_WithInvalidPage_ThrowsArgumentException(
      int page)
  {
    var service = await CreateServiceAsync();

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new PatientSearchQuery(
                Page: page)));
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  public async Task ExecuteAsync_WithInvalidPageSize_ThrowsArgumentException(
      int pageSize)
  {
    var service = await CreateServiceAsync();

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new PatientSearchQuery(
                PageSize: pageSize)));
  }

  [Fact]
  public async Task ExecuteAsync_WithPageSizeOverOneHundred_ThrowsArgumentException()
  {
    var service = await CreateServiceAsync();

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new PatientSearchQuery(
                PageSize: 101)));
  }

  [Fact]
  public async Task ExecuteAsync_TrimsSearchInput()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            Search: "  Alice  "));

    Assert.Single(result.Items);
    Assert.Equal("Alice", result.Items[0].FirstName);
  }

  [Fact]
  public async Task ExecuteAsync_CalculatesTotalPagesUsingCeilingDivision()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            PageSize: 3));

    Assert.Equal(2, result.TotalPages);
  }

  private static async Task<SearchPatientsService> CreateServiceAsync()
  {
    var repository = new FakePatientRepository();

    var patients = new[]
    {
            new Patient(
                "PAT-001",
                "Alice",
                "Anderson",
                new DateOnly(1985, 1, 1)),

            new Patient(
                "PAT-002",
                "Bob",
                "Smith",
                new DateOnly(1986, 1, 1)),

            new Patient(
                "PAT-003",
                "Carol",
                "Smith",
                new DateOnly(1987, 1, 1)),

            new Patient(
                "PAT-004",
                "David",
                "Zimmer",
                new DateOnly(1988, 1, 1))
        };

    foreach (var patient in patients)
    {
      await repository.AddAsync(patient);
    }

    return new SearchPatientsService(repository);
  }

  [Fact]
  public async Task ExecuteAsync_SortsByFirstNameDescending()
  {
    var service = await CreateServiceAsync();

    var result = await service.ExecuteAsync(
        new PatientSearchQuery(
            SortBy: "firstName",
            SortDirection: "desc"));

    Assert.Equal(
        ["David", "Carol", "Bob", "Alice"],
        result.Items.Select(patient => patient.FirstName));
  }

  [Fact]
  public async Task ExecuteAsync_WithInvalidSortField_ThrowsArgumentException()
  {
    var service = await CreateServiceAsync();

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new PatientSearchQuery(
                SortBy: "invalidField")));
  }

  [Fact]
  public async Task ExecuteAsync_WithInvalidSortDirection_ThrowsArgumentException()
  {
    var service = await CreateServiceAsync();

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.ExecuteAsync(
            new PatientSearchQuery(
                SortDirection: "sideways")));
  }
}