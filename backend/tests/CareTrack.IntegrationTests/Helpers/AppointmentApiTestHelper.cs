using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Appointments;
using CareTrack.IntegrationTests.Contracts.Referrals;

namespace CareTrack.IntegrationTests.Helpers;

public static class AppointmentApiTestHelper
{
  public static async Task<AppointmentResponse>
      CreateAppointmentAsync(
          HttpClient client,
          Guid patientId,
          Guid referralId,
          AppointmentType appointmentType =
              AppointmentType.Consultation)
  {
    var start =
        DateTime.UtcNow.AddDays(2);

    var request =
        new
        {
          appointmentReference =
                $"APT-{Guid.NewGuid():N}"[..12],

          patientId,

          referralId,

          appointmentType,

          scheduledStart =
                start,

          scheduledEnd =
                start.AddMinutes(30),

          location =
                "Birmingham Clinic"
        };

    var response =
        await client.PostAsJsonAsync(
            "/api/appointments",
            request);

    if (response.StatusCode !=
        HttpStatusCode.Created)
    {
      var body =
          await response.Content
              .ReadAsStringAsync();

      throw new InvalidOperationException(
          $"Appointment creation failed. " +
          $"Status: {response.StatusCode}. " +
          $"Body: {body}");
    }

    var appointment =
        await response.Content
            .ReadFromJsonAsync<
                AppointmentResponse>();

    return appointment
        ?? throw new InvalidOperationException(
            "Appointment response was empty.");
  }
}