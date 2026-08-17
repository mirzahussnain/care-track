using System.Net;
using System.Net.Http.Json;
using CareTrack.IntegrationTests.Contracts.ClinicalNotes;

namespace CareTrack.IntegrationTests.Helpers;

public static class ClinicalNoteApiTestHelper
{
  public static async Task<ClinicalNoteResponse>
      CreateClinicalNoteAsync(
          HttpClient client,
          Guid appointmentId,
          string content = "Patient reports improving symptoms.",
          string createdBy = "clinician.demo")
  {
    var request =
        new
        {
          content,
          createdBy
        };

    var response =
        await client.PostAsJsonAsync(
            $"/api/appointments/{appointmentId}/clinical-notes",
            request);

    if (response.StatusCode != HttpStatusCode.Created)
    {
      var body =
          await response.Content.ReadAsStringAsync();

      throw new InvalidOperationException(
          $"Clinical note creation failed. " +
          $"Status: {response.StatusCode}. " +
          $"Body: {body}");
    }

    return await response.Content
        .ReadFromJsonAsync<ClinicalNoteResponse>()
        ?? throw new InvalidOperationException(
            "Clinical note response was empty.");
  }
}