using System.Net;
using System.Net.Http.Json;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Infrastructure;
namespace CareTrack.IntegrationTests.Patients;

public class SearchPatientsTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public SearchPatientsTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
    _client = factory.CreateClient();
  }
  public async Task InitializeAsync()
  {
    await _factory.ResetDatabaseAsync();
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  private async Task CreatePatientAsync(
      string patientReference,
      string firstName,
      string lastName,
      string dateOfBirth = "1990-01-01")
  {
    var request = new
    {
      patientReference,
      firstName,
      lastName,
      dateOfBirth
    };

    var response =
        await _client.PostAsJsonAsync(
            "/api/patients",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);
  }


  [Fact]
  public async Task GetPatients_WithFirstName_Descending_ReturnsCorrectOrder()
  {
    // Arrange
    var prefix =
        Guid.NewGuid()
            .ToString("N")[..6];
    var sharedLastName = $"Sort{prefix}";
    await CreatePatientAsync($"PAT-{prefix}-1", "Alice", sharedLastName);
    await CreatePatientAsync($"PAT-{prefix}-2", "Carol", sharedLastName);
    await CreatePatientAsync($"PAT-{prefix}-3", "Bob", sharedLastName);

    //Act
    var response = await _client.GetAsync(
    $"/api/patients" + $"?search={sharedLastName}" + $"&sortBy=firstName" + $"&sortDirection=desc" + $"&page=1" + $"&pageSize=20");

    //Assert
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    var result = await response.Content.ReadFromJsonAsync<PagedPatientResponse>();
    Assert.NotNull(result);
    Assert.Equal(
    ["Carol", "Bob", "Alice"],

result.Items.Select(patient => patient.FirstName));
  }

  [Fact]
  public async Task GetPatients_WithPatientReferenceAscending_ReturnsCorrectOrder()
  {
    // Arrange
    var prefix =
        Guid.NewGuid()
            .ToString("N")[..6];

    var sharedLastName = $"Reference{prefix}";

    await CreatePatientAsync(
        $"PAT-{prefix}-C",
        "Alice",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-A",
        "Bob",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-B",
        "Carol",
        sharedLastName);

    // Act
    var response = await _client.GetAsync(
        $"/api/patients" +
        $"?search={sharedLastName}" +
        $"&sortBy=patientReference" +
        $"&sortDirection=asc");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<PagedPatientResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        [
            $"PAT-{prefix}-A",
            $"PAT-{prefix}-B",
            $"PAT-{prefix}-C"
        ],
        result.Items.Select(
            patient => patient.PatientReference));
  }
  [Fact]
  public async Task GetPatients_WithInvalidSortField_ReturnsBadRequest()
  {
    // Act
    var response = await _client.GetAsync(
        "/api/patients" +
        "?sortBy=invalidField" +
        "&sortDirection=asc");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }
  [Fact]
  public async Task GetPatients_WithInvalidSortDirection_ReturnsBadRequest()
  {
    // Act
    var response = await _client.GetAsync(
        "/api/patients" +
        "?sortBy=lastName" +
        "&sortDirection=sideways");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task GetPatients_WithMixedCaseSortParameters_AppliesNormalizedSorting()
  {
    // Arrange
    var prefix =
        Guid.NewGuid()
            .ToString("N")[..6];

    var sharedLastName = $"Normalize{prefix}";

    await CreatePatientAsync(
        $"PAT-{prefix}-1",
        "Alice",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-2",
        "Charlie",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-3",
        "Bob",
        sharedLastName);

    // Act
    var response = await _client.GetAsync(
        $"/api/patients" +
        $"?search={sharedLastName}" +
        $"&sortBy=FirstName" +
        $"&sortDirection=DESC");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<PagedPatientResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        ["Charlie", "Bob", "Alice"],
        result.Items.Select(
            patient => patient.FirstName));
  }
  [Fact]
  public async Task GetPatients_WithSortingAndPagination_ReturnsCorrectPage()
  {
    // Arrange
    var prefix =
        Guid.NewGuid()
            .ToString("N")[..6];

    var sharedLastName = $"Paged{prefix}";

    await CreatePatientAsync(
        $"PAT-{prefix}-A",
        "Alice",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-B",
        "Bob",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-C",
        "Carol",
        sharedLastName);

    await CreatePatientAsync(
        $"PAT-{prefix}-D",
        "David",
        sharedLastName);

    // Act
    var response = await _client.GetAsync(
        $"/api/patients" +
        $"?search={sharedLastName}" +
        $"&sortBy=firstName" +
        $"&sortDirection=asc" +
        $"&page=2" +
        $"&pageSize=2");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<PagedPatientResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        ["Carol", "David"],
        result.Items.Select(
            patient => patient.FirstName));

    Assert.Equal(4, result.TotalCount);
    Assert.Equal(2, result.TotalPages);
    Assert.Equal(2, result.Page);
    Assert.Equal(2, result.PageSize);
  }

  [Fact]
  public async Task SearchPagedPatient_WithQuery_ReturnsMatchingPatients()
  {
    // Arrange
    var uniqueReference = $"PAT-{Guid.NewGuid():N}"[..12];

    var request = new
    {
      patientReference = uniqueReference,
      firstName = "Alice",
      lastName = "Smith",
      dateOfBirth = "1990-05-20"
    };

    var createResponse = await _client.PostAsJsonAsync(
        "/api/patients",
        request);

    Assert.Equal(
        HttpStatusCode.Created,
        createResponse.StatusCode);

    // Act
    var response = await _client.GetAsync(
        "/api/patients?search=Smith&page=1&pageSize=20");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<PagedPatientResponse>();

    Assert.NotNull(result);

    Assert.Contains(
        result.Items,
        patient => patient.PatientReference == uniqueReference);
  }
}