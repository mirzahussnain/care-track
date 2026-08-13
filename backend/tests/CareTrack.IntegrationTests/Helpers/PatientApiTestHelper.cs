using System.Net;
using System.Net.Http.Json;
using CareTrack.IntegrationTests.Contracts.Patients;

namespace CareTrack.IntegrationTests.Helpers;

public static class PatientApiTestHelper
{
  public static async Task<PatientResponse>
      CreatePatientAsync(
          HttpClient client,
          string firstName = "John",
          string lastName = "Smith",
          string dateOfBirth = "1990-05-20")
  {
    var patientReference =
        $"PAT-{Guid.NewGuid():N}"[..12];

    var request = new
    {
      patientReference,
      firstName,
      lastName,
      dateOfBirth
    };

    var response =
        await client.PostAsJsonAsync(
            "/api/patients",
            request);

    if (response.StatusCode !=
        HttpStatusCode.Created)
    {
      var body =
          await response.Content
              .ReadAsStringAsync();

      throw new InvalidOperationException(
          $"Patient creation failed. " +
          $"Status: {response.StatusCode}. " +
          $"Body: {body}");
    }

    var patient =
        await response.Content
            .ReadFromJsonAsync<PatientResponse>();

    return patient
        ?? throw new InvalidOperationException(
            "Patient response was empty.");
  }
}