using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;

namespace CareTrack.IntegrationTests.Authentication;

public sealed class RouteAuthorizationTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public RouteAuthorizationTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  [Fact]
  public async Task
      GetPatientById_WhenAnonymous_ReturnsUnauthorized()
  {
    using var client =
        _factory.CreateClient();

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task
      GetPatientById_WithReferralCoordinator_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      GetPatientById_WithClinician_ReachesApplication()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    var response =
        await client.GetAsync(
            $"/api/patients/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WhenAnonymous_ReturnsUnauthorized()
  {
    using var client =
        _factory.CreateClient();

    var response =
        await client.PostAsJsonAsync(
            "/api/appointments",
            CreateInvalidAppointmentRequest());

    Assert.Equal(
        HttpStatusCode.Unauthorized,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WithAdministratorOnly_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.AdministratorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Administrator);

    var response =
        await client.PostAsJsonAsync(
            "/api/appointments",
            CreateInvalidAppointmentRequest());

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CreateAppointment_WithReferralCoordinator_ReachesApplication()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var response =
        await client.PostAsJsonAsync(
            "/api/appointments",
            CreateInvalidAppointmentRequest());

    Assert.NotEqual(
        HttpStatusCode.Unauthorized,
        response.StatusCode);

    Assert.NotEqual(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CheckInAppointment_WithReferralCoordinator_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var response =
        await client.PostAsync(
            $"/api/appointments/{Guid.NewGuid()}/check-in",
            content: null);

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      CheckInAppointment_WithClinician_ReachesApplication()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    var response =
        await client.PostAsync(
            $"/api/appointments/{Guid.NewGuid()}/check-in",
            content: null);

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }


  [Fact]
  public async Task
    GetClinicalNote_WithReferralCoordinator_ReturnsForbidden()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ReferralCoordinatorId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.ReferralCoordinator);

    var response =
        await client.GetAsync(
            $"/api/clinical-notes/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.Forbidden,
        response.StatusCode);
  }

  [Fact]
  public async Task
      GetClinicalNote_WithClinician_ReachesApplication()
  {
    using var client =
        TestAuthenticatedClient.Create(
            _factory,
            TestUsers.ClinicianId,
            CareTrackScopes.AccessAsUser,
            CareTrackRoles.Clinician);

    var response =
        await client.GetAsync(
            $"/api/clinical-notes/{Guid.NewGuid()}");

    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }


  private static object CreateInvalidAppointmentRequest()
  {
    var start =
        DateTime.UtcNow.AddDays(1);

    return new
    {
      appointmentReference =
            $"AUTH-{Guid.NewGuid():N}"[..12],

      patientId =
            Guid.NewGuid(),

      referralId =
            Guid.NewGuid(),

      appointmentType =
            AppointmentType.Consultation,

      scheduledStart =
            start,

      scheduledEnd =
            start.AddMinutes(30),

      location =
            "Authorization Test"
    };
  }

}