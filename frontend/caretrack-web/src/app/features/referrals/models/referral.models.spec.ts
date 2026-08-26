import {
  REFERRAL_PRIORITIES,
  REFERRAL_STATUSES,
  referralPriorityLabel,
  referralStatusLabel,
  referralStatusTone,
} from './referral.models';

describe('referral presentation', () => {
  it('preserves the backend numeric enum values', () => {
    expect(REFERRAL_STATUSES.draft).toBe(0);
    expect(REFERRAL_STATUSES.completed).toBe(8);
    expect(REFERRAL_STATUSES.cancelled).toBe(10);
    expect(REFERRAL_PRIORITIES.routine).toBe(0);
    expect(REFERRAL_PRIORITIES.urgent).toBe(1);
  });

  it('maps status and priority into readable text and semantic tones', () => {
    expect(referralStatusLabel(REFERRAL_STATUSES.awaitingTriage)).toBe('Awaiting triage');
    expect(referralStatusTone(REFERRAL_STATUSES.awaitingTriage)).toBe('warning');
    expect(referralStatusTone(REFERRAL_STATUSES.rejected)).toBe('danger');
    expect(referralPriorityLabel(REFERRAL_PRIORITIES.urgent)).toBe('Urgent');
  });
});
