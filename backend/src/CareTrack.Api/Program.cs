using CareTrack.Api.Authorization;
using CareTrack.Api.ErrorHandling;
using CareTrack.Api.Health;
using CareTrack.Api.Identity;
using CareTrack.Api.Observability;
using CareTrack.Application.Appointments.CancelAppointment;
using CareTrack.Application.Appointments.CheckInAppointment;
using CareTrack.Application.Appointments.CompleteAppointment;
using CareTrack.Application.Appointments.CreateAppointment;
using CareTrack.Application.Appointments.DidNotAttendAppointment;
using CareTrack.Application.Appointments.GetAppointmentById;
using CareTrack.Application.Appointments.SearchAppointments;
using CareTrack.Application.Appointments.StartAppointment;
using CareTrack.Application.ClinicalNotes.CreateClinicalNote;
using CareTrack.Application.ClinicalNotes.GetClinicalNoteById;
using CareTrack.Application.ClinicalNotes.GetClinicalNotesByAppointment;
using CareTrack.Application.ClinicalNotes.UpdateClinicalNote;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.Application.Patients.GetPatient;
using CareTrack.Application.Patients.SearchPatients;
using CareTrack.Application.Patients.UpdatePatient;
using CareTrack.Application.Referrals.AcceptReferral;
using CareTrack.Application.Referrals.AssignReferral;
using CareTrack.Application.Referrals.CompleteReferral;
using CareTrack.Application.Referrals.CreateReferral;
using CareTrack.Application.Referrals.GetReferralById;
using CareTrack.Application.Referrals.GetReferralHistory;
using CareTrack.Application.Referrals.ReassignReferral;
using CareTrack.Application.Referrals.RecordTriageAssessment;
using CareTrack.Application.Referrals.RejectReferral;
using CareTrack.Application.Referrals.RequestMoreInformation;
using CareTrack.Application.Referrals.ResubmitReferral;
using CareTrack.Application.Referrals.SearchReferrals;
using CareTrack.Application.Referrals.StartTriage;
using CareTrack.Application.Referrals.SubmitReferral;
using CareTrack.Infrastructure.Configuration;
using CareTrack.Infrastructure.Persistance;
using CareTrack.Infrastructure.Persistance.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);
const string FrontendCorsPolicy = "Frontend";
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CareTrackDbContext>(options =>
{
  options.UseSqlServer(
      builder.Configuration.GetConnectionString("CareTrack"),
      sqlServerOptions =>
          sqlServerOptions.EnableRetryOnFailure(
              maxRetryCount: 3,
              maxRetryDelay: TimeSpan.FromSeconds(5),
              errorNumbersToAdd: null));
});
builder.Services.AddHealthChecks()
    .AddCheck<CareTrackDatabaseHealthCheck>("database", tags: ["ready"]);
builder.Services.AddScoped<IApplicationTransaction, ApplicationTransaction>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IClinicalNoteRepository, ClinicalNoteRepository>();
var referralAssignmentTargets = builder.Configuration
    .GetSection("ReferralAssignment:Targets")
    .GetChildren()
    .Select(section => section.Value ?? string.Empty)
    .ToArray();
builder.Services.AddSingleton<IReferralAssignmentTargetDirectory>(
    new ConfiguredReferralAssignmentTargetDirectory(
        referralAssignmentTargets));
builder.Services.AddScoped<CreatePatientService>();
builder.Services.AddScoped<GetPatientService>();
builder.Services.AddScoped<SearchPatientsService>();
builder.Services.AddScoped<UpdatePatientService>();
builder.Services.AddScoped<CreateReferralService>();
builder.Services.AddScoped<SubmitReferralService>();
builder.Services.AddScoped<StartTriageService>();
builder.Services.AddScoped<AcceptReferralService>();
builder.Services.AddScoped<RejectReferralService>();
builder.Services.AddScoped<RequestMoreInformationService>();
builder.Services.AddScoped<ResubmitReferralService>();
builder.Services.AddScoped<RecordTriageAssessmentService>();
builder.Services.AddScoped<AssignReferralService>();
builder.Services.AddScoped<ReassignReferralService>();
builder.Services.AddScoped<CompleteReferralService>();
builder.Services.AddScoped<GetReferralHistoryService>();
builder.Services.AddScoped<GetReferralByIdService>();
builder.Services.AddScoped<SearchReferralsService>();
builder.Services.AddScoped<CreateAppointmentService>();
builder.Services.AddScoped<CheckInAppointmentService>();
builder.Services.AddScoped<StartAppointmentService>();
builder.Services.AddScoped<CompleteAppointmentService>();
builder.Services.AddScoped<CancelAppointmentService>();
builder.Services.AddScoped<MarkAppointmentDidNotAttendService>();
builder.Services.AddScoped<GetAppointmentByIdService>();
builder.Services.AddScoped<SearchAppointmentsService>();
builder.Services.AddScoped<CreateClinicalNoteService>();
builder.Services.AddScoped<GetClinicalNoteByIdService>();
builder.Services.AddScoped<GetClinicalNotesByAppointmentService>();
builder.Services.AddScoped<UpdateClinicalNoteService>();
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization(options =>
{
  options.AddPolicy(
      CareTrackPolicies.ClinicianAccess,
      policy =>
      {
        policy.RequireAuthenticatedUser();
        policy.RequireScope(CareTrackScopes.AccessAsUser);
        policy.RequireRole(CareTrackRoles.Clinician);
      });

  options.AddPolicy(CareTrackPolicies.ReferralManagement,
  policy =>
  {
    policy.RequireAuthenticatedUser();
    policy.RequireScope(CareTrackScopes.AccessAsUser);
    policy.RequireRole(CareTrackRoles.ReferralCoordinator, CareTrackRoles.Clinician);
  });

  options.AddPolicy(CareTrackPolicies.AdministrativeAccess,
  policy =>
  {
    policy.RequireAuthenticatedUser();
    policy.RequireScope(CareTrackScopes.AccessAsUser);
    policy.RequireRole(CareTrackRoles.Administrator);
  });

  options.AddPolicy(
    CareTrackPolicies.ApiAccess,
    policy =>
    {
      policy.RequireAuthenticatedUser();
      policy.RequireScope(CareTrackScopes.AccessAsUser);
    }

  );

});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();
builder.Services.AddSingleton<IDemoAccountDirectory, DemoAccountDirectory>();
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
  options.AddPolicy(FrontendCorsPolicy, policy =>
  {
    policy
          .WithOrigins(allowedOrigins)
          .AllowAnyHeader()
          .AllowAnyMethod();
  });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler();
app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();



app.MapGet("/api/health", () =>
{
  return Results.Ok(new
  {
    status = "healthy",
    service = "CareTrack.Api"
  });
})
.AllowAnonymous();

app.MapHealthChecks(
    "/api/health/ready",
    new HealthCheckOptions
    {
      Predicate = registration =>
          registration.Tags.Contains("ready"),
      ResultStatusCodes =
      {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status503ServiceUnavailable,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
      },
      ResponseWriter = HealthCheckResponseWriter.WriteAsync
    })
    .AllowAnonymous();

app.Run();



public partial class Program;