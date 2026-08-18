using CareTrack.Api.ErrorHandling;
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
using CareTrack.Infrastructure.Persistance;
using CareTrack.Infrastructure.Persistance.Repositories;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<CareTrackDbContext>(options =>
{
  options.UseSqlServer(
  builder.Configuration.GetConnectionString("CareTrack")
  );
});
builder.Services.AddScoped<IApplicationTransaction, ApplicationTransaction>();
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<IReferralRepository, ReferralRepository>();
builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IClinicalNoteRepository, ClinicalNoteRepository>();
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
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
  var forecast = Enumerable.Range(1, 5).Select(index =>
      new WeatherForecast
      (
          DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
          Random.Shared.Next(-20, 55),
          summaries[Random.Shared.Next(summaries.Length)]
      ))
      .ToArray();
  return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/api/health", () =>
{
  return Results.Ok(new
  {
    status = "healthy",
    service = "CareTrack.Api"
  });
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
  public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}


public partial class Program;