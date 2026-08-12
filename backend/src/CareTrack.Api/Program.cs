using CareTrack.Api.ErrorHandling;
using CareTrack.Application.Common.Interfaces;
using CareTrack.Application.Patients.CreatePatient;
using CareTrack.Application.Patients.GetPatient;
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
builder.Services.AddScoped<IPatientRepository, PatientRepository>();
builder.Services.AddScoped<CreatePatientService>();
builder.Services.AddScoped<GetPatientService>();
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
