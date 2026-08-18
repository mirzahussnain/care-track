
using System.Net;
using System.Net.Http.Json;
using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Common.Models;
using CareTrack.Domain.Entities;
using CareTrack.Domain.Enums;
using CareTrack.Infrastructure.Persistance;
using CareTrack.Infrastructure.Persistance.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Helpers;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Appointments;

public class CreateAppointmentTests :
    IClassFixture<
        CareTrackSqlServerWebApplicationFactory>,
    IAsyncLifetime
{
  private readonly HttpClient _client;

  private readonly
      CareTrackSqlServerWebApplicationFactory
      _factory;

  public CreateAppointmentTests(
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

  [Fact]
  public async Task
    CreateAppointment_WithValidRequest_ReturnsCreated()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-001",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var appointment =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    Assert.NotNull(
        appointment);

    Assert.Equal(
        "APT-INT-001",
        appointment.AppointmentReference);

    Assert.Equal(
        patient.Id,
        appointment.PatientId);

    Assert.Equal(
        referral.Id,
        appointment.ReferralId);

    Assert.Equal(
        AppointmentStatus.Scheduled,
        appointment.Status);
  }

  [Fact]
  public async Task
      CreateAppointment_WithUnknownPatient_ReturnsNotFound()
  {
    // Arrange
    var unknownPatientId =
        Guid.NewGuid();

    var unknownReferralId =
        Guid.NewGuid();

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-002",

          patientId =
                unknownPatientId,

          referralId =
                unknownReferralId,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithUnknownReferral_ReturnsNotFound()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-003",

          patientId =
                patient.Id,

          referralId =
                Guid.NewGuid(),

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WhenReferralBelongsToDifferentPatient_ReturnsBadRequest()
  {
    // Arrange
    var patientOne =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client,
                "John",
                "Smith");

    var patientTwo =
        await PatientApiTestHelper
            .CreatePatientAsync(
                _client,
                "Jane",
                "Smith");

    var referral =
        await ReferralApiTestHelper
            .CreateReferralAsync(
                _client,
                patientOne.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-004",

          // Deliberately Patient Two
          patientId =
                patientTwo.Id,

          // But this belongs to Patient One
          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithDuplicateReference_ReturnsConflict()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-DUP-001",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    // Act
    var secondResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.Conflict,
        secondResponse.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithWhitespaceNormalizedDuplicateReference_ReturnsConflict()
  {
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                _client,
                passedPatientId: patient.Id);

    var appointmentReference =
        $"APT-{Guid.NewGuid():N}"[..12];

    var start =
        DateTime.UtcNow.AddDays(2);

    var firstResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            new
            {
              appointmentReference,
              patientId = patient.Id,
              referralId = referral.Id,
              appointmentType = AppointmentType.Consultation,
              scheduledStart = start,
              scheduledEnd = start.AddMinutes(30),
              location = "Birmingham Clinic"
            });

    Assert.Equal(
        HttpStatusCode.Created,
        firstResponse.StatusCode);

    var duplicateResponse =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            new
            {
              appointmentReference = $" {appointmentReference} ",
              patientId = patient.Id,
              referralId = referral.Id,
              appointmentType = AppointmentType.Consultation,
              scheduledStart = start.AddDays(1),
              scheduledEnd = start.AddDays(1).AddMinutes(30),
              location = "Birmingham Clinic"
            });

    Assert.Equal(
        HttpStatusCode.Conflict,
        duplicateResponse.StatusCode);
  }

  [Fact]
  public async Task
    CreateAppointment_WithConcurrentOverlappingRequests_OnlyOneIsPersisted()
  {
    var barrier =
        new SchedulingCheckBarrier();

    using var concurrentFactory =
        _factory.WithWebHostBuilder(
            builder =>
                builder.ConfigureServices(
                    services =>
                    {
                      services.RemoveAll<IAppointmentRepository>();
                      services.AddScoped<AppointmentRepository>();
                      services.AddScoped<IAppointmentRepository>(
                          provider =>
                              new CoordinatedAppointmentRepository(
                                  provider.GetRequiredService<AppointmentRepository>(),
                                  barrier));
                    }));

    using var firstClient =
        concurrentFactory.CreateClient();

    using var secondClient =
        concurrentFactory.CreateClient();

    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(firstClient);

    var firstReferral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                firstClient,
                passedPatientId: patient.Id);

    var secondReferral =
        await ReferralApiTestHelper
            .CreateAssignedReferralAsync(
                firstClient,
                passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var firstRequest =
        new
        {
          appointmentReference = $"APT-{Guid.NewGuid():N}"[..12],
          patientId = patient.Id,
          referralId = firstReferral.Id,
          appointmentType = AppointmentType.Consultation,
          scheduledStart = start,
          scheduledEnd = start.AddMinutes(30),
          location = "Birmingham Clinic"
        };

    var secondRequest =
        new
        {
          appointmentReference = $"APT-{Guid.NewGuid():N}"[..12],
          patientId = patient.Id,
          referralId = secondReferral.Id,
          appointmentType = AppointmentType.Consultation,
          scheduledStart = start.AddMinutes(10),
          scheduledEnd = start.AddMinutes(40),
          location = "Birmingham Clinic"
        };

    var responses =
        await Task.WhenAll(
            firstClient.PostAsJsonAsync(
                "/api/appointments",
                firstRequest),
            secondClient.PostAsJsonAsync(
                "/api/appointments",
                secondRequest));

    Assert.Equal(
        1,
        responses.Count(
            response =>
                response.StatusCode == HttpStatusCode.Created));

    Assert.Equal(
        1,
        responses.Count(
            response =>
                response.StatusCode == HttpStatusCode.Conflict));

    using var scope =
        concurrentFactory.Services.CreateScope();

    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<CareTrackDbContext>();

    var persistedAppointments =
        await dbContext.Appointments
            .CountAsync(
                appointment =>
                    appointment.PatientId == patient.Id
                    && appointment.ScheduledStart < start.AddMinutes(40)
                    && appointment.ScheduledEnd > start);

    Assert.Equal(
        1,
        persistedAppointments);
  }

  [Fact]
  public async Task
    CreateAppointment_WhenEndIsBeforeStart_ReturnsBadRequest()
  {
    // Arrange
    var patient =
        await PatientApiTestHelper
            .CreatePatientAsync(_client);

    var referral =
       await ReferralApiTestHelper
        .CreateAssignedReferralAsync(
            _client,
            passedPatientId: patient.Id);

    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                "APT-INT-005",

          patientId =
                patient.Id,

          referralId =
                referral.Id,

          appointmentType =
                AppointmentType.Consultation,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(-30),

          location =
                "Birmingham Clinic"
        };

    // Act
    var response =
        await _client.PostAsJsonAsync(
            "/api/appointments",
            request);

    // Assert
    Assert.Equal(
        HttpStatusCode.BadRequest,
        response.StatusCode);
  }


  private sealed class CoordinatedAppointmentRepository
      : IAppointmentRepository
  {
    private readonly IAppointmentRepository _inner;
    private readonly SchedulingCheckBarrier _barrier;

    public CoordinatedAppointmentRepository(
        IAppointmentRepository inner,
        SchedulingCheckBarrier barrier)
    {
      _inner = inner;
      _barrier = barrier;
    }

    public Task<Appointment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
      return _inner.GetByIdAsync(id, cancellationToken);
    }

    public Task<Appointment?> GetByReferenceAsync(
        string appointmentReference,
        CancellationToken cancellationToken = default)
    {
      return _inner.GetByReferenceAsync(
          appointmentReference,
          cancellationToken);
    }

    public Task<IReadOnlyList<Appointment>> GetByReferralIdAsync(
        Guid referralId,
        CancellationToken cancellationToken = default)
    {
      return _inner.GetByReferralIdAsync(
          referralId,
          cancellationToken);
    }

    public Task<PagedResult<Appointment>> SearchAsync(
        AppointmentSearchCommand query,
        CancellationToken cancellationToken = default)
    {
      return _inner.SearchAsync(query, cancellationToken);
    }

    public async Task<bool> HasSchedulingConflictAsync(
        Guid patientId,
        DateTime scheduledStart,
        DateTime scheduledEnd,
        Guid? excludeAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
      var hasConflict =
          await _inner.HasSchedulingConflictAsync(
              patientId,
              scheduledStart,
              scheduledEnd,
              excludeAppointmentId,
              cancellationToken);

      if (!hasConflict)
      {
        await _barrier.WaitAsync(cancellationToken);
      }

      return hasConflict;
    }

    public Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
      return _inner.AddAsync(appointment, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
      return _inner.SaveChangesAsync(cancellationToken);
    }
  }

  private sealed class SchedulingCheckBarrier
  {
    private readonly TaskCompletionSource _release =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private int _arrivals;

    public Task WaitAsync(
        CancellationToken cancellationToken)
    {
      if (Interlocked.Increment(ref _arrivals) == 2)
      {
        _release.TrySetResult();
      }

      return _release.Task.WaitAsync(
          TimeSpan.FromSeconds(15),
          cancellationToken);
    }
  }

}