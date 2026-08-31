using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Appointments;

public sealed class AppointmentSearchTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory
      _factory;


  public AppointmentSearchTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
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
  public async Task
      GetAppointmentById_WhenAppointmentExists_ReturnsOk()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral.Id);

    // Act
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments/{appointment.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<AppointmentResponse>();

    Assert.NotNull(result);

    Assert.Equal(
        appointment.Id,
        result.Id);

    Assert.Equal(
        appointment.AppointmentReference,
        result.AppointmentReference);

    Assert.Equal(
        patient.Id,
        result.PatientId);

    Assert.Equal(
        referral.Id,
        result.ReferralId);
  }

  [Fact]
  public async Task
      GetAppointmentById_WhenAppointmentDoesNotExist_ReturnsNotFound()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Act
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments/{Guid.NewGuid()}");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByPatientId_ReturnsOnlyPatientAppointments()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient1 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var patient2 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient1.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient2.Id);

    var start =
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(9);

    var patient1Appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient1.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            referralCoordinatorClient,
            patient2.Id,
            referral2.Id,
            scheduledStart:
                start.AddHours(1),
            scheduledEnd:
                start.AddHours(1)
                    .AddMinutes(30));

    // Act
    _factory.CommandRecorder.Clear();
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments?patientId={patient1.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        patient1Appointment.Id,
        result.Items[0].Id);

    Assert.Equal(patient1.PatientReference, result.Items[0].PatientReference);
    Assert.Equal(patient1.FullName, result.Items[0].PatientDisplayName);
    Assert.Equal(referral1.ReferralReference, result.Items[0].ReferralReference);

    var listCommands = _factory.CommandRecorder.CommandTexts;
    Assert.Equal(2, listCommands.Count);
    Assert.All(
        listCommands,
        sql => Assert.Contains("[vw_AppointmentOperationalList]", sql));

    Assert.All(
        result.Items,
        item =>
            Assert.Equal(
                patient1.Id,
                item.PatientId));
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByReferralId_ReturnsOnlyReferralAppointments()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(9);

    var expected =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            referralCoordinatorClient,
            patient.Id,
            referral2.Id,
            scheduledStart:
                start.AddHours(1),
            scheduledEnd:
                start.AddHours(1)
                    .AddMinutes(30));

    // Act
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments?referralId={referral1.Id}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        expected.Id,
        result.Items[0].Id);

    Assert.Equal(
        referral1.Id,
        result.Items[0].ReferralId);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByStatus_ReturnsOnlyMatchingStatus()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow
            .AddDays(5)
            .Date
            .AddHours(9);

    var scheduled =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30));

    var cancelled =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral2.Id,
                scheduledStart:
                    start.AddHours(1),
                scheduledEnd:
                    start.AddHours(1)
                        .AddMinutes(30));

    var cancelResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{cancelled.Id}/cancel",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        cancelResponse.StatusCode);

    // Act
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments?status={(int)AppointmentStatus.Cancelled}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        cancelled.Id,
        result.Items[0].Id);

    Assert.Equal(
        AppointmentStatus.Cancelled,
        result.Items[0].Status);

    Assert.DoesNotContain(
        result.Items,
        item =>
            item.Id == scheduled.Id);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByAppointmentType_ReturnsMatchingAppointments()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow
            .AddDays(6)
            .Date
            .AddHours(9);

    var consultation =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient.Id,
                referral1.Id,
                AppointmentType.Consultation,
                start,
                start.AddMinutes(30));

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            referralCoordinatorClient,
            patient.Id,
            referral2.Id,
            AppointmentType.FollowUp,
            start.AddHours(1),
            start.AddHours(1)
                .AddMinutes(30));

    // Act
    var response =
        await clinicianClient.GetAsync(
            $"/api/appointments?appointmentType={(int)AppointmentType.Consultation}");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        consultation.Id,
        result.Items[0].Id);

    Assert.Equal(
        AppointmentType.Consultation,
        result.Items[0].AppointmentType);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByLocation_ReturnsPartialLocationMatches()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient1 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var patient2 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient1.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient2.Id);

    var start =
        DateTime.UtcNow
            .AddDays(6)
            .Date
            .AddHours(10);

    var expected =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient1.Id,
                referral1.Id,
                scheduledStart: start,
                scheduledEnd:
                    start.AddMinutes(30),
                location:
                    "Birmingham Clinic");

    await AppointmentApiTestHelper
        .CreateAppointmentAsync(
            referralCoordinatorClient,
            patient2.Id,
            referral2.Id,
            scheduledStart:
                start.AddHours(1),
            scheduledEnd:
                start.AddHours(1)
                    .AddMinutes(30),
            location:
                "Coventry Clinic");

    // Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?location=Birmingham");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        expected.Id,
        result.Items[0].Id);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenFilteringByTimeWindow_ReturnsIntersectingAppointments()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var patient1 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var patient2 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var patient3 =
        await PatientApiTestHelper
            .CreatePatientAsync(
                referralCoordinatorClient);

    var referral1 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient1.Id);

    var referral2 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient2.Id);

    var referral3 =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                referralCoordinatorClient,
                "Integration Test Team",
                passedPatientId: patient3.Id);

    var day =
        DateTime.UtcNow
            .AddDays(7)
            .Date;

    var appointmentA =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient1.Id,
                referral1.Id,
                scheduledStart:
                    day.AddHours(9),
                scheduledEnd:
                    day.AddHours(9)
                        .AddMinutes(30));

    var appointmentB =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient2.Id,
                referral2.Id,
                scheduledStart:
                    day.AddHours(9)
                        .AddMinutes(45),
                scheduledEnd:
                    day.AddHours(10)
                        .AddMinutes(15));

    var appointmentC =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                referralCoordinatorClient,
                patient3.Id,
                referral3.Id,
                scheduledStart:
                    day.AddHours(11),
                scheduledEnd:
                    day.AddHours(11)
                        .AddMinutes(30));

    var from =
        day.AddHours(10);

    var to =
        day.AddHours(11);

    var url =
        $"/api/appointments" +
        $"?scheduledFrom={Uri.EscapeDataString(from.ToString("O"))}" +
        $"&scheduledTo={Uri.EscapeDataString(to.ToString("O"))}";

    // Act
    var response =
        await clinicianClient.GetAsync(url);

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Single(
        result.Items);

    Assert.Equal(
        appointmentB.Id,
        result.Items[0].Id);

    Assert.DoesNotContain(
        result.Items,
        item =>
            item.Id == appointmentA.Id);

    Assert.DoesNotContain(
        result.Items,
        item =>
            item.Id == appointmentC.Id);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenPaginated_ReturnsCorrectMetadata()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var start =
        DateTime.UtcNow
            .AddDays(8)
            .Date
            .AddHours(8);

    for (var i = 0; i < 5; i++)
    {
      var patient =
          await PatientApiTestHelper
              .CreatePatientAsync(
                  referralCoordinatorClient);

      var referral =
          await ReferralApiTestHelper
              .CreateAssignedReferralAsync(
                  referralCoordinatorClient,
                  "Integration Test Team",
                  passedPatientId: patient.Id);

      await AppointmentApiTestHelper
          .CreateAppointmentAsync(
              referralCoordinatorClient,
              patient.Id,
              referral.Id,
              scheduledStart:
                  start.AddHours(i),
              scheduledEnd:
                  start.AddHours(i)
                      .AddMinutes(30));
    }

    // Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?page=2&pageSize=2");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    Assert.Equal(
        2,
        result.Items.Count);

    Assert.Equal(
        2,
        result.Page);

    Assert.Equal(
        2,
        result.PageSize);

    Assert.Equal(
        5,
        result.TotalCount);

    Assert.Equal(
        3,
        result.TotalPages);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenSortedByScheduledStartAscending_ReturnsAscendingOrder()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var day =
        DateTime.UtcNow
            .AddDays(9)
            .Date;

    var starts =
        new[]
        {
                day.AddHours(11),
                day.AddHours(9),
                day.AddHours(10)
        };

    foreach (var start in starts)
    {
      var patient =
          await PatientApiTestHelper
              .CreatePatientAsync(
                  referralCoordinatorClient);

      var referral =
          await ReferralApiTestHelper
              .CreateAssignedReferralAsync(
                  referralCoordinatorClient,
                  "Integration Test Team",
                  passedPatientId: patient.Id);

      await AppointmentApiTestHelper
          .CreateAppointmentAsync(
              referralCoordinatorClient,
              patient.Id,
              referral.Id,
              scheduledStart: start,
              scheduledEnd:
                  start.AddMinutes(30));
    }

    // Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments" +
            "?sortBy=scheduledStart" +
            "&sortDirection=asc");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    var actual =
        result.Items
            .Select(
                item =>
                    item.ScheduledStart)
            .ToList();

    var expected =
        actual
            .OrderBy(
                value => value)
            .ToList();

    Assert.Equal(
        expected,
        actual);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenSortedByScheduledStartDescending_ReturnsDescendingOrder()
  {
        using var referralCoordinatorClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var day =
        DateTime.UtcNow
            .AddDays(10)
            .Date;

    var starts =
        new[]
        {
                day.AddHours(9),
                day.AddHours(11),
                day.AddHours(10)
        };

    foreach (var start in starts)
    {
      var patient =
          await PatientApiTestHelper
              .CreatePatientAsync(
                  referralCoordinatorClient);

      var referral =
          await ReferralApiTestHelper
              .CreateAssignedReferralAsync(
                  referralCoordinatorClient,
                  "Integration Test Team",
                  passedPatientId: patient.Id);

      await AppointmentApiTestHelper
          .CreateAppointmentAsync(
              referralCoordinatorClient,
              patient.Id,
              referral.Id,
              scheduledStart: start,
              scheduledEnd:
                  start.AddMinutes(30));
    }

    // Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments" +
            "?sortBy=scheduledStart" +
            "&sortDirection=desc");

    // Assert
    Assert.Equal(
        HttpStatusCode.OK,
        response.StatusCode);

    var result =
        await response.Content
            .ReadFromJsonAsync<
                PagedResult<AppointmentSearchItemResponse>>();

    Assert.NotNull(result);

    var actual =
        result.Items
            .Select(
                item =>
                    item.ScheduledStart)
            .ToList();

    var expected =
        actual
            .OrderByDescending(
                value => value)
            .ToList();

    Assert.Equal(
        expected,
        actual);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenPageIsInvalid_ReturnsBadRequest()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?page=0");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenPageSizeExceedsMaximum_ReturnsBadRequest()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?pageSize=101");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenTimeRangeIsInvalid_ReturnsBadRequest()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Arrange
    var from =
        DateTime.UtcNow.AddDays(5);

    var to =
        from.AddHours(-1);

    var url =
        $"/api/appointments" +
        $"?scheduledFrom={Uri.EscapeDataString(from.ToString("O"))}" +
        $"&scheduledTo={Uri.EscapeDataString(to.ToString("O"))}";

    // Act
    var response =
        await clinicianClient.GetAsync(url);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenSortFieldIsInvalid_ReturnsBadRequest()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?sortBy=banana");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
      SearchAppointments_WhenSortDirectionIsInvalid_ReturnsBadRequest()
  {
        using var clinicianClient =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

// Act
    var response =
        await clinicianClient.GetAsync(
            "/api/appointments?sortDirection=sideways");

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }
}
