using System.Net;
using System.Net.Http.Json;
using CareTrack.Api.Authorization;
using CareTrack.Domain.Enums;
using CareTrack.Infrastructure.Persistance;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;
using CareTrack.IntegrationTests.Infrastructure.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CareTrack.IntegrationTests.Appointments;

public sealed class TransactionRetrySafetyTests
    : IClassFixture<CareTrackSqlServerWebApplicationFactory>,
      IAsyncLifetime
{
  private readonly CareTrackSqlServerWebApplicationFactory _factory;

  public TransactionRetrySafetyTests(
      CareTrackSqlServerWebApplicationFactory factory)
  {
    _factory = factory;
  }

  public Task InitializeAsync()
  {
    return _factory.ResetDatabaseAsync();
  }

  public Task DisposeAsync()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public async Task CreateAppointment_TransientFailureBeforeCommit_RetriesOnceWithoutDuplicates()
  {
    var setup = await CreateAssignedReferralAsync();
    var faultState =
        new CommitFaultState(
            CommitFaultMode.TransientBeforeCommit);

    using var faultFactory =
        CreateFaultFactory(faultState);
    using var client =
        CreateReferralCoordinatorClient(faultFactory);

    var appointmentReference =
        $"RETRY-{Guid.NewGuid():N}"[..18];

    var response =
        await SendCreateAppointmentAsync(
            client,
            setup.PatientId,
            setup.ReferralId,
            appointmentReference);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.Equal(2, faultState.CommitAttempts);

    await AssertSingleCreateOutcomeAsync(
        setup.ReferralId,
        appointmentReference);
  }

  [Fact]
  public async Task CreateAppointment_AmbiguousFailureAfterCommit_VerifiesWithoutReplay()
  {
    var setup = await CreateAssignedReferralAsync();
    var faultState =
        new CommitFaultState(
            CommitFaultMode.TransientAfterCommit);

    using var faultFactory =
        CreateFaultFactory(faultState);
    using var client =
        CreateReferralCoordinatorClient(faultFactory);

    var appointmentReference =
        $"AMBIG-{Guid.NewGuid():N}"[..18];

    var response =
        await SendCreateAppointmentAsync(
            client,
            setup.PatientId,
            setup.ReferralId,
            appointmentReference);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    Assert.Equal(1, faultState.CommitAttempts);

    await AssertSingleCreateOutcomeAsync(
        setup.ReferralId,
        appointmentReference);
  }

  [Fact]
  public async Task StartAppointment_TransientFailureBeforeCommit_RetriesOnceWithoutDuplicateHistory()
  {
    var setup = await CreateCheckedInAppointmentAsync();
    var faultState =
        new CommitFaultState(
            CommitFaultMode.TransientBeforeCommit);

    using var faultFactory =
        CreateFaultFactory(faultState);
    using var client =
        CreateClinicianClient(faultFactory);

    var response =
        await client.PostAsync(
            $"/api/appointments/{setup.AppointmentId}/start",
            content: null);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(2, faultState.CommitAttempts);

    await AssertSingleStartOutcomeAsync(
        setup.AppointmentId,
        setup.ReferralId);
  }

  [Fact]
  public async Task StartAppointment_AmbiguousFailureAfterCommit_VerifiesExactStartedAtWithoutReplay()
  {
    var setup = await CreateCheckedInAppointmentAsync();
    var faultState =
        new CommitFaultState(
            CommitFaultMode.TransientAfterCommit);

    using var faultFactory =
        CreateFaultFactory(faultState);
    using var client =
        CreateClinicianClient(faultFactory);

    var response =
        await client.PostAsync(
            $"/api/appointments/{setup.AppointmentId}/start",
            content: null);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(1, faultState.CommitAttempts);

    var returned =
        await response.Content
            .ReadFromJsonAsync<AppointmentResponse>();

    Assert.NotNull(returned);

    using var scope = _factory.Services.CreateScope();
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<CareTrackDbContext>();

    var persisted =
        await dbContext.Appointments
            .AsNoTracking()
            .SingleAsync(
                appointment =>
                    appointment.Id == setup.AppointmentId);

    Assert.Equal(returned!.StartedAt, persisted.StartedAt);

    await AssertSingleStartOutcomeAsync(
        setup.AppointmentId,
        setup.ReferralId);
  }

  [Fact]
  public async Task CreateAppointment_NonTransientFailureBeforeCommit_RollsBackCompletely()
  {
    var setup = await CreateAssignedReferralAsync();
    var faultState =
        new CommitFaultState(
            CommitFaultMode.NonTransientBeforeCommit);

    using var faultFactory =
        CreateFaultFactory(faultState);
    using var client =
        CreateReferralCoordinatorClient(faultFactory);

    var appointmentReference =
        $"ROLLBACK-{Guid.NewGuid():N}"[..18];

    var response =
        await SendCreateAppointmentAsync(
            client,
            setup.PatientId,
            setup.ReferralId,
            appointmentReference);

    Assert.Equal(
        HttpStatusCode.InternalServerError,
        response.StatusCode);
    Assert.Equal(1, faultState.CommitAttempts);

    using var scope = _factory.Services.CreateScope();
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<CareTrackDbContext>();

    Assert.False(
        await dbContext.Appointments
            .AsNoTracking()
            .AnyAsync(
                appointment =>
                    appointment.AppointmentReference ==
                    appointmentReference));

    var referral =
        await dbContext.Referrals
            .AsNoTracking()
            .SingleAsync(
                referral =>
                    referral.Id == setup.ReferralId);

    Assert.Equal(ReferralStatus.Assigned, referral.Status);

    Assert.Equal(
        0,
        await CountReferralHistoryAsync(
            dbContext,
            setup.ReferralId,
            ReferralHistoryEventType.Scheduled));
  }

  private async Task<(Guid PatientId, Guid ReferralId)>
      CreateAssignedReferralAsync()
  {
    using var client =
        CreateReferralCoordinatorClient(_factory);

    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                client,
                passedPatientId: patient.Id);

    return (patient.Id, referral.Id);
  }

  private async Task<(
      Guid AppointmentId,
      Guid ReferralId)> CreateCheckedInAppointmentAsync()
  {
    using var coordinatorClient =
        CreateReferralCoordinatorClient(_factory);
    using var clinicianClient =
        CreateClinicianClient(_factory);

    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(coordinatorClient);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                coordinatorClient,
                passedPatientId: patient.Id);

    var appointment =
        await AppointmentApiTestHelper
            .CreateAppointmentAsync(
                coordinatorClient,
                patient.Id,
                referral.Id);

    var checkInResponse =
        await clinicianClient.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            content: null);

    Assert.Equal(HttpStatusCode.OK, checkInResponse.StatusCode);

    return (appointment.Id, referral.Id);
  }

  private async Task AssertSingleCreateOutcomeAsync(
      Guid referralId,
      string appointmentReference)
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<CareTrackDbContext>();

    Assert.Equal(
        1,
        await dbContext.Appointments
            .AsNoTracking()
            .CountAsync(
                appointment =>
                    appointment.AppointmentReference ==
                    appointmentReference));

    var referral =
        await dbContext.Referrals
            .AsNoTracking()
            .SingleAsync(
                referral => referral.Id == referralId);

    Assert.Equal(ReferralStatus.Scheduled, referral.Status);

    Assert.Equal(
        1,
        await CountReferralHistoryAsync(
            dbContext,
            referralId,
            ReferralHistoryEventType.Scheduled));
  }

  private async Task AssertSingleStartOutcomeAsync(
      Guid appointmentId,
      Guid referralId)
  {
    using var scope = _factory.Services.CreateScope();
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<CareTrackDbContext>();

    var appointment =
        await dbContext.Appointments
            .AsNoTracking()
            .SingleAsync(
                appointment => appointment.Id == appointmentId);

    Assert.Equal(AppointmentStatus.InProgress, appointment.Status);
    Assert.NotNull(appointment.StartedAt);

    var referral =
        await dbContext.Referrals
            .AsNoTracking()
            .SingleAsync(
                referral => referral.Id == referralId);

    Assert.Equal(ReferralStatus.InProgress, referral.Status);

    Assert.Equal(
        1,
        await CountReferralHistoryAsync(
            dbContext,
            referralId,
            ReferralHistoryEventType.Started));
  }

  private static Task<int> CountReferralHistoryAsync(
      CareTrackDbContext dbContext,
      Guid referralId,
      ReferralHistoryEventType eventType)
  {
    return dbContext.ReferralHistoryEntries
        .AsNoTracking()
        .CountAsync(
            entry =>
                entry.ReferralId == referralId
                && entry.EventType == eventType);
  }

  private WebApplicationFactory<Program> CreateFaultFactory(
      CommitFaultState faultState)
  {
    return _factory.WithWebHostBuilder(
        builder =>
            builder.ConfigureServices(services =>
            {
              services.RemoveAll<
                  IDbContextOptionsConfiguration<
                      CareTrackDbContext>>();

              var configuration =
                  services.BuildServiceProvider()
                      .GetRequiredService<IConfiguration>();

              var connectionString =
                  configuration.GetConnectionString(
                      "IntegrationDatabase")
                  ?? throw new InvalidOperationException(
                      "Integration database connection string was not found.");

              var interceptor =
                  new CommitFaultInterceptor(faultState);

              services.AddDbContext<CareTrackDbContext>(options =>
              {
                options.UseSqlServer(
                    connectionString,
                    sqlServerOptions =>
                        sqlServerOptions.ExecutionStrategy(
                            dependencies =>
                                new TestExecutionStrategy(
                                    dependencies)));

                options.AddInterceptors(interceptor);
              });
            }));
  }

  private static HttpClient CreateReferralCoordinatorClient(
      WebApplicationFactory<Program> factory)
  {
    return TestAuthenticatedClient.Create(
        factory,
        TestUsers.ReferralCoordinatorId,
        CareTrackScopes.AccessAsUser,
        CareTrackRoles.ReferralCoordinator);
  }

  private static HttpClient CreateClinicianClient(
      WebApplicationFactory<Program> factory)
  {
    return TestAuthenticatedClient.Create(
        factory,
        TestUsers.ClinicianId,
        CareTrackScopes.AccessAsUser,
        CareTrackRoles.Clinician);
  }

  private static Task<HttpResponseMessage>
      SendCreateAppointmentAsync(
          HttpClient client,
          Guid patientId,
          Guid referralId,
          string appointmentReference)
  {
    var start = DateTime.UtcNow.AddDays(2);

    return client.PostAsJsonAsync(
        "/api/appointments",
        new
        {
          appointmentReference,
          patientId,
          referralId,
          appointmentType = AppointmentType.Consultation,
          scheduledStart = start,
          scheduledEnd = start.AddMinutes(30),
          location = "Retry Safety Clinic"
        });
  }
}
