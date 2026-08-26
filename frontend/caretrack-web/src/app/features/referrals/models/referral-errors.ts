import { HttpErrorResponse } from '@angular/common/http';

import { ProblemDetails } from './referral.models';

export type ReferralCommandError =
  | 'validation'
  | 'forbidden'
  | 'not-found'
  | 'workflow'
  | 'conflict'
  | 'concurrency'
  | 'generic';

export interface ReferralErrorMessage {
  readonly kind: ReferralCommandError;
  readonly message: string;
}

export function referralErrorMessage(
  error: HttpErrorResponse,
  fallback: string,
): ReferralErrorMessage {
  const problem = error.error as ProblemDetails | null;

  if (error.status === 403) {
    return { kind: 'forbidden', message: 'You are not permitted to perform this action.' };
  }
  if (error.status === 404) {
    return { kind: 'not-found', message: problem?.detail || 'The requested record was not found.' };
  }
  if (error.status === 400) {
    return { kind: 'validation', message: problem?.detail || 'Check the submitted information.' };
  }
  if (error.status === 409 && problem?.title === 'Concurrency Conflict') {
    return {
      kind: 'concurrency',
      message: problem.detail || 'This record changed while you were working. Reload and try again.',
    };
  }
  if (error.status === 409 && problem?.title === 'Invalid State Transition') {
    return {
      kind: 'workflow',
      message: problem.detail || 'This action is no longer allowed in the current workflow state.',
    };
  }
  if (error.status === 409) {
    return { kind: 'conflict', message: problem?.detail || 'The request conflicts with current data.' };
  }

  return { kind: 'generic', message: fallback };
}
