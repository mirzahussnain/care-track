using System.Net;
using System.Net.Http.Json;
using CareTrack.Domain.Enums;
using CareTrack.IntegrationTests.Contracts.Referrals;

namespace CareTrack.IntegrationTests.Helpers;

public static class ReferralApiTestHelper
{
  public static async Task<ReferralResponse>
      CreateReferralAsync(
          HttpClient client,
          Guid? passedPatientId = null)
  {
    Guid patientId;

    if (passedPatientId.HasValue)
    {
      patientId = passedPatientId.Value;
    }
    else
    {
      var patient =
          await PatientApiTestHelper
              .CreatePatientAsync(client);

      patientId = patient.Id;
    }

    var request = new
    {
      referralReference =
            $"REF-{Guid.NewGuid():N}"[..12],

      patientId,

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

  public static async Task<ReferralResponse>
      CreateReferralWithPriorityAsync(
          HttpClient client,
          ReferralPriority passedPriority =
              ReferralPriority.Routine,
          Guid? passedPatientId = null)
  {
    Guid patientId;

    if (passedPatientId.HasValue)
    {
      patientId =
          passedPatientId.Value;
    }
    else
    {
      var patient =
          await PatientApiTestHelper
              .CreatePatientAsync(client);

      patientId =
          patient.Id;
    }

    var request = new
    {
      referralReference =
            $"REF-{Guid.NewGuid():N}"[..12],

      patientId,

      priority =
            passedPriority,

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

  public static async Task
      CreateSeveralReferralsAsync(
          HttpClient client,
          Guid passedPatientId,
          int count)
  {
    for (var i = 0; i < count; i++)
    {
      await CreateReferralAsync(
          client,
          passedPatientId);
    }
  }

  public static async Task<ReferralResponse>
      CreateAcceptedReferralAsync(
          HttpClient client,
          ReferralPriority? passedPriority = null,
          Guid? passedPatientId = null)
  {
    ReferralResponse referral;

    if (passedPriority.HasValue)
    {
      referral =
          await CreateReferralWithPriorityAsync(
              client,
              passedPriority.Value,
              passedPatientId);
    }
    else
    {
      referral =
          await CreateReferralAsync(
              client,
              passedPatientId);
    }

    // Draft -> Submitted
    var submitResponse =
        await client.PostAsync(
            $"/api/referrals/{referral.Id}/submit",
            null);

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

  public static async Task<ReferralResponse>
      CreateAssignedReferralAsync(
          HttpClient client,
          string assignedTo = ReferralTestAssignmentTargets.Default,
          ReferralPriority? passedPriority = null,
          Guid? passedPatientId = null)
  {
    var referral =
        await CreateAcceptedReferralAsync(
            client,
            passedPriority,
            passedPatientId);

    var assignResponse =
        await client.PostAsJsonAsync(
            $"/api/referrals/{referral.Id}/assign",
            new
            {
              assignedTo
            });

    Assert.Equal(
        HttpStatusCode.OK,
        assignResponse.StatusCode);

    var assignedReferral =
        await assignResponse.Content
            .ReadFromJsonAsync<ReferralResponse>();

    Assert.NotNull(
        assignedReferral);

    Assert.Equal(
        ReferralStatus.Assigned,
        assignedReferral.Status);

    Assert.Equal(
        assignedTo,
        assignedReferral.AssignedTo);

    return assignedReferral;
  }
}