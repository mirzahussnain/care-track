import { HttpErrorResponse } from '@angular/common/http';

import { referralErrorMessage } from './referral-errors';

describe('referralErrorMessage', () => {
  it('distinguishes concurrency by Problem Details title', () => {
    const result = referralErrorMessage(
      new HttpErrorResponse({
        status: 409,
        error: {
          title: 'Concurrency Conflict',
          detail: 'The record changed.',
        },
      }),
      'Fallback',
    );
    expect(result).toEqual({ kind: 'concurrency', message: 'The record changed.' });
  });

  it('distinguishes invalid workflow transitions from concurrency', () => {
    const result = referralErrorMessage(
      new HttpErrorResponse({
        status: 409,
        error: {
          title: 'Invalid State Transition',
          detail: 'Only draft referrals can be submitted.',
        },
      }),
      'Fallback',
    );
    expect(result).toEqual({
      kind: 'workflow',
      message: 'Only draft referrals can be submitted.',
    });
  });

  it('keeps ordinary 409 responses as endpoint-specific conflicts', () => {
    const result = referralErrorMessage(
      new HttpErrorResponse({
        status: 409,
        error: {
          title: 'Conflict',
          detail: 'No appointment has been completed.',
        },
      }),
      'Fallback',
    );
    expect(result).toEqual({
      kind: 'conflict',
      message: 'No appointment has been completed.',
    });
  });
});
