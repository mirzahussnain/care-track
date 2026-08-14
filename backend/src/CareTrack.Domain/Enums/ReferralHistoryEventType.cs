namespace CareTrack.Domain.Enums;

public enum ReferralHistoryEventType
{
  Created = 0,
  Submitted = 1,
  TriageStarted = 2,
  MoreInformationRequested = 3,
  Resubmitted = 4,
  Accepted = 5,
  Rejected = 6,
  TriageAssessmentRecorded = 7,
  Assigned = 8,
  Reassigned = 9
}