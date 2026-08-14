using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Patients;
using CareTrack.IntegrationTests.Contracts.Referrals;
using CareTrack.IntegrationTests.Infrastructure;

namespace CareTrack.IntegrationTests.Helpers;

public class ReferralApiTestHelper
{
  public static async Task<ReferralResponse> CreateReferralAsync(HttpClient client)
  {
    var patient = await PatientApiTestHelper.CreatePatientAsync(client);

    var request = new
    {
      referralReference =
            $"REF-{Guid.NewGuid():N}"[..12],

      patientId =
            patient.Id,

      priority =
            ReferralPriority.Routine,

      reason =
            "Persistent shoulder pain."
    };

    var response =
        await client.PostAsJsonAsync(
            "/api/referrals",
            request);

    Assert.Equal(
        HttpStatusCode.Created,
        response.StatusCode);

    var referral =
        await response.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(referral);

    return referral;
  }
  public static async Task<ReferralResponse> CreateAcceptedReferralAsync(HttpClient client)
  {
    var referral = await CreateReferralAsync(client);

    //Draft -> Submitted
    var submitResponse = await client.PostAsync($"/api/referrals/{referral.Id}/submit", null);

    Assert.Equal(
      HttpStatusCode.OK,
      submitResponse.StatusCode);

    // Submitted -> AwaitingTriage

    var startTriageResponse =
      await client.PostAsync(
          $"/api/referrals/{referral.Id}/start-triage",
          null);

    Assert.Equal(
      HttpStatusCode.OK,
      startTriageResponse.StatusCode);

    // AwaitingTriage -> Accepted
    var acceptResponse =
        await client.PostAsync(
            $"/api/referrals/{referral.Id}/accept",
            null);

    Assert.Equal(
        HttpStatusCode.OK,
        acceptResponse.StatusCode);


    var acceptedReferral =
        await acceptResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        acceptedReferral);

    Assert.Equal(
        ReferralStatus.Accepted,
        acceptedReferral.Status);

    return acceptedReferral;


  }

}

