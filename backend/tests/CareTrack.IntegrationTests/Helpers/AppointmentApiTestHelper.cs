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
              AppointmentType.Consultation,
               DateTime? scheduledStart = null,
    DateTime? scheduledEnd = null,
    string location = "Birmingham Clinic")
  {
    var start =
        scheduledStart
        ?? DateTime.UtcNow.AddDays(5);

    var end =
        scheduledEnd
        ?? start.AddMinutes(30);
    var request =
        new
        {
          appointmentReference =
                $"APT-{Guid.NewGuid():N}"[..12],

          patientId,

          referralId,

          appointmentType =
            (int)appointmentType,

          scheduledStart =
                start,

          scheduledEnd =
            end,

          location
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

  public static Task<HttpResponseMessage> SendCreateAppointmentRequestAsync(
    HttpClient client,
    Guid patientId,
    Guid referralId,
    AppointmentType appointmentType = AppointmentType.Consultation,
    DateTime? scheduledStart = null,
    DateTime? scheduledEnd = null,
    string location = "Birmingham Clinic")
  {
    var start =
        scheduledStart
        ?? DateTime.UtcNow.AddDays(5);

    var end =
        scheduledEnd
        ?? start.AddMinutes(30);

    var request = new
    {
      appointmentReference =
            $"APT-{Guid.NewGuid():N}"[..16],

      patientId,

      referralId,

      appointmentType =
            (int)appointmentType,

      scheduledStart =
            start,

      scheduledEnd =
            end,

      location
    };

    return client.PostAsJsonAsync(
        "/api/appointments",
        request);
  }

  public static async Task<AppointmentResponse>
    CreateCheckedInAppointmentAsync(
        HttpClient client,
        Guid patientId,
        Guid referralId)
  {
    var appointment =
        await CreateAppointmentAsync(
            client,
            patientId,
            referralId);

    var response =
        await client.PostAsync(
            $"/api/appointments/{appointment.Id}/check-in",
            null);

    if (!response.IsSuccessStatusCode)
    {
      var body =
          await response.Content
              .ReadAsStringAsync();

      throw new InvalidOperationException(
          $"Appointment check-in failed. " +
          $"Status: {response.StatusCode}. " +
          $"Body: {body}");
    }

    return await response.Content
        .ReadFromJsonAsync<AppointmentResponse>()
        ?? throw new InvalidOperationException(
            "Appointment response was empty.");
  }

  public static async Task<AppointmentResponse>
    CreateInProgressAppointmentAsync(
        HttpClient client,
        Guid patientId,
        Guid referralId)
  {
    var appointment =
        await CreateCheckedInAppointmentAsync(
            client,
            patientId,
            referralId);

    var response =
        await client.PostAsync(
            $"/api/appointments/{appointment.Id}/start",
            null);

    if (!response.IsSuccessStatusCode)
    {
      var body =
          await response.Content
              .ReadAsStringAsync();

      throw new InvalidOperationException(
          $"Appointment start failed. " +
          $"Status: {response.StatusCode}. " +
          $"Body: {body}");
    }

    return await response.Content
        .ReadFromJsonAsync<AppointmentResponse>()
        ?? throw new InvalidOperationException(
            "Appointment response was empty.");
  }
}