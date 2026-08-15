using System.Net;
using System.Net.Http.Json;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Referrals;

public class SearchReferralsTests : IClassFixture<CareTrackSqlServerWebApplicationFactory>, IAsyncLifetime
{
  private readonly HttpClient _client;
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public SearchReferralsTests(CareTrackSqlServerWebApplicationFactory factory)
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

  [Fact]
  public async Task GetReferralById_WhenReferralExists_ReturnsReferral()
  {
    // Arrange

    var referral = await ReferralApiTestHelper.CreateReferralAsync(_client);

    // Act
    var response = await _client.GetAsync(
        $"/api/referrals/{referral.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result = await response.Content
        .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        referral.Id,
        result.Id);

    Assert.Equal(
        referral.ReferralReference,
        result.ReferralReference);

    Assert.Equal(
        referral.PatientId,
        result.PatientId);

    Assert.Equal(
        ReferralStatus.Draft,
        result.Status);

    Assert.Equal(
        ReferralPriority.Routine,
        result.Priority);
  }

  [Fact]
  public async Task GetReferralById_WhenReferralDoesNotExist_ReturnsNotFound()
  {
    // Arrange
    var unknownReferralId = Guid.NewGuid();

    // Act
    var response = await _client.GetAsync(
        $"/api/referrals/{unknownReferralId}");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  // =========================================================
  // PAGINATION
  // =========================================================

  [Fact]
  public async Task SearchReferrals_WhenPageSizeIsFive_ReturnsFiveItems()
  {
    // Arrange
    var patient = await PatientApiTestHelper.CreatePatientAsync(_client);

    await ReferralApiTestHelper.CreateSeveralReferralsAsync(
    _client,
    patient.Id,
        8);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&page=1" +
        "&pageSize=5" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Equal(
        5,
        result.Items.Count);

    Assert.Equal(
        8,
        result.TotalCount);

    Assert.Equal(
        1,
        result.Page);

    Assert.Equal(
        5,
        result.PageSize);

    Assert.Equal(
        2,
        result.TotalPages);
  }

  [Fact]
  public async Task SearchReferrals_WhenRequestingSecondPage_ReturnsRemainingItems()
  {
    // Arrange
    var patient = await PatientApiTestHelper.CreatePatientAsync(_client);

    await ReferralApiTestHelper.CreateSeveralReferralsAsync(
    _client,
        patient.Id,
        8);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&page=2" +
        "&pageSize=5" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Equal(
        3,
        result.Items.Count);

    Assert.Equal(
        8,
        result.TotalCount);

    Assert.Equal(
        2,
        result.Page);

    Assert.Equal(
        5,
        result.PageSize);

    Assert.Equal(
        2,
        result.TotalPages);
  }

  // =========================================================
  // STATUS FILTER
  // =========================================================

  [Fact]
  public async Task SearchReferrals_FilterByStatus_ReturnsOnlyMatchingReferrals()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var draftReferral =
        await ReferralApiTestHelper
            .CreateReferralWithPriorityAsync(
                _client,
                ReferralPriority.Routine,
                patient.Id);

    var submittedReferral =
        await ReferralApiTestHelper
            .CreateReferralWithPriorityAsync(
                _client,
                ReferralPriority.Routine,
                patient.Id);

    var submitResponse =
        await _client.PostAsync(
            $"/api/referrals/{submittedReferral.Id}/submit",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        submitResponse.StatusCode);

    // Act
    var result =
        await _client.GetFromJsonAsync<
            PagedResult<ReferralResponse>>(
            $"/api/referrals" +
            $"?patientId={patient.Id}" +
            "&status=Submitted" +
            "&page=1" +
            "&pageSize=20" +
            "&sortBy=createdat" +
            "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Single(result.Items);

    Assert.Equal(
        submittedReferral.Id,
        result.Items[0].Id);

    Assert.Equal(
        ReferralStatus.Submitted,
        result.Items[0].Status);

    Assert.DoesNotContain(
        result.Items,
        referral =>
            referral.Id == draftReferral.Id);
  }

  // =========================================================
  // PRIORITY FILTER
  // =========================================================

  [Fact]
  public async Task SearchReferrals_FilterByPriority_ReturnsOnlyMatchingReferrals()
  {
    // Arrange

    var patient =
     await PatientApiTestHelper
         .CreatePatientAsync(_client);

    await ReferralApiTestHelper
        .CreateReferralWithPriorityAsync(
            _client,
            ReferralPriority.Routine,
            patient.Id);

    var urgentReferral =
        await ReferralApiTestHelper
            .CreateReferralWithPriorityAsync(
                _client,
                ReferralPriority.Urgent,
                patient.Id);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={urgentReferral.PatientId}" +
        "&priority=Urgent" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        urgentReferral.Id,
        result.Items[0].Id);

    Assert.Equal(
        ReferralPriority.Urgent,
        result.Items[0].Priority);
  }

  // =========================================================
  // PATIENT FILTER
  // =========================================================

  [Fact]
  public async Task SearchReferrals_FilterByPatientId_ReturnsOnlyPatientsReferrals()
  {
    // Arrange
    var patientA = await PatientApiTestHelper.CreatePatientAsync(_client);
    var patientB = await PatientApiTestHelper.CreatePatientAsync(_client);

    await ReferralApiTestHelper.CreateSeveralReferralsAsync(
        _client, patientA.Id, 3);

    await ReferralApiTestHelper.CreateSeveralReferralsAsync(
    _client,
        patientB.Id,
        2);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patientA.Id}" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Equal(
        3,
        result.TotalCount);

    Assert.Equal(
        3,
        result.Items.Count);

    Assert.All(
        result.Items,
        referral =>
            Assert.Equal(
                patientA.Id,
                referral.PatientId));
  }

  // =========================================================
  // ASSIGNED TO FILTER
  // =========================================================

  [Fact]
  public async Task SearchReferrals_FilterByAssignedTo_ReturnsCurrentAssignment()
  {
    // Arrange
    var patient =
    await PatientApiTestHelper
        .CreatePatientAsync(_client);

    var teamAReferral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team A",
                passedPatientId: patient.Id);

    await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            "Cardiology Team B",
            passedPatientId: patient.Id);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&assignedTo=Cardiology%20Team%20A" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        teamAReferral.Id,
        result.Items[0].Id);

    Assert.Equal(
        "Cardiology Team A",
        result.Items[0].AssignedTo);
  }

  // =========================================================
  // CURRENT ASSIGNMENT VS HISTORY
  // =========================================================

  [Fact]
  public async Task SearchReferrals_AfterReassignment_UsesCurrentAssignment()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team A",
                ReferralPriority.Urgent,
                patient.Id);

    var reassignResponse = await _client.PostAsJsonAsync(
        $"/api/referrals/{referral.Id}/reassign",
        new
        {
          assignedTo = "Cardiology Team B"
        });

    Assert.Equal(
        HttpStatusCode.OK,
        reassignResponse.StatusCode);

    // Act - search old assignment
    var teamAResult = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&assignedTo=Cardiology%20Team%20A" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Act - search current assignment
    var teamBResult = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&assignedTo=Cardiology%20Team%20B" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(teamAResult);
    Assert.NotNull(teamBResult);

    Assert.DoesNotContain(
        teamAResult.Items,
        item =>
            item.Id == referral.Id);

    Assert.Contains(
        teamBResult.Items,
        item =>
            item.Id == referral.Id);
  }

  // =========================================================
  // COMBINED FILTERS
  // =========================================================

  [Fact]
  public async Task SearchReferrals_WithCombinedFilters_ReturnsOnlyMatchingReferral()
  {
    // Arrange
    var patient =
    await PatientApiTestHelper
        .CreatePatientAsync(_client);

    var matchingReferral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                "Cardiology Team A",
                ReferralPriority.Urgent,
                patient.Id);

    // Same assignment, wrong priority
    await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            "Cardiology Team A",
            ReferralPriority.Routine,
            patient.Id);

    // Same priority, wrong assignment
    await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            "Cardiology Team B",
            ReferralPriority.Urgent,
            patient.Id);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={matchingReferral.PatientId}" +
        "&status=Assigned" +
        "&priority=Urgent" +
        "&assignedTo=Cardiology%20Team%20A" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        matchingReferral.Id,
        result.Items[0].Id);

    Assert.Equal(
        ReferralStatus.Assigned,
        result.Items[0].Status);

    Assert.Equal(
        ReferralPriority.Urgent,
        result.Items[0].Priority);

    Assert.Equal(
        "Cardiology Team A",
        result.Items[0].AssignedTo);
  }

  // =========================================================
  // DATE FILTER
  // =========================================================

  [Fact]
  public async Task SearchReferrals_FilterByCreatedDate_ReturnsReferralCreatedToday()
  {
    // Arrange


    var referral = await ReferralApiTestHelper.CreateReferralAsync(
        _client);

    var today = DateOnly.FromDateTime(
        DateTime.UtcNow);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={referral.PatientId}" +
        $"&createdFrom={today:yyyy-MM-dd}" +
        $"&createdTo={today:yyyy-MM-dd}" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.NotNull(result);

    Assert.Contains(
        result.Items,
        item =>
            item.Id == referral.Id);
  }

  // =========================================================
  // SORTING
  // =========================================================

  [Fact]
  public async Task SearchReferrals_SortByReferralReferenceAscending_ReturnsSortedResults()
  {
    // Arrange
    var patient = await PatientApiTestHelper.CreatePatientAsync(_client);

    await ReferralApiTestHelper.CreateReferralAsync(_client, patient.Id);

    await ReferralApiTestHelper.CreateReferralAsync(
        _client, patient.Id);

    await ReferralApiTestHelper.CreateReferralAsync(
        _client, patient.Id);

    // Act
    var result = await _client.GetFromJsonAsync<
        PagedResult<ReferralResponse>>(
        $"/api/referrals" +
        $"?patientId={patient.Id}" +
        "&page=1" +
        "&pageSize=20" +
        "&sortBy=referralreference" +
        "&sortDirection=asc");

    // Assert
    Assert.NotNull(result);

    var returnedReferences = result.Items
        .Select(
            referral =>
                referral.ReferralReference)
        .ToList();

    var expectedReferences = returnedReferences
        .OrderBy(
            reference =>
                reference)
        .ToList();

    Assert.Equal(
        expectedReferences,
        returnedReferences);
  }

  // =========================================================
  // INVALID PAGINATION
  // =========================================================

  [Theory]
  [InlineData(0, 20)]
  [InlineData(1, 0)]
  [InlineData(1, 101)]
  public async Task SearchReferrals_WithInvalidPagination_ReturnsBadRequest(
      int page,
      int pageSize)
  {
    // Act
    var response = await _client.GetAsync(
        $"/api/referrals" +
        $"?page={page}" +
        $"&pageSize={pageSize}" +
        "&sortBy=createdat" +
        "&sortDirection=desc");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  // =========================================================
  // INVALID SORT FIELD
  // =========================================================

  [Fact]
  public async Task SearchReferrals_WithInvalidSortField_ReturnsBadRequest()
  {
    // Act
    var response = await _client.GetAsync(
        "/api/referrals" +
        "?page=1" +
        "&pageSize=20" +
        "&sortBy=banana" +
        "&sortDirection=desc");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  // =========================================================
  // INVALID SORT DIRECTION
  // =========================================================

  [Fact]
  public async Task SearchReferrals_WithInvalidSortDirection_ReturnsBadRequest()
  {
    // Act
    var response = await _client.GetAsync(
        "/api/referrals" +
        "?page=1" +
        "&pageSize=20" +
        "&sortBy=createdat" +
        "&sortDirection=sideways");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }


}