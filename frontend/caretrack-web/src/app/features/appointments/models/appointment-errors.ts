import { HttpErrorResponse } from '@angular/common/http';

export interface AppointmentProblemDetails {
  readonly status?: number;
  readonly title?: string;
  readonly detail?: string;
}

export type AppointmentErrorKind =
  | 'authentication'
  | 'forbidden'
  | 'not-found'
  | 'validation'
  | 'overlap'
  | 'duplicate-reference'
  | 'referral-state'
  | 'workflow'
  | 'conflict'
  | 'generic';

export interface AppointmentErrorMessage {
  readonly kind: AppointmentErrorKind;
  readonly message: string;
}

export function appointmentErrorMessage(
  error: HttpErrorResponse,
  fallback: string,
): AppointmentErrorMessage {
  const problem = (error.error ?? {}) as AppointmentProblemDetails;
  const title = problem.title?.toLowerCase() ?? '';
  const detail = problem.detail?.toLowerCase() ?? '';

  if (error.status === 401) {
    return {
      kind: 'authentication',
      message: 'Your session could not be verified. Sign in again.',
    };
  }
  if (error.status === 403) {
    return { kind: 'forbidden', message: 'Your role does not permit this appointment action.' };
  }
  if (error.status === 404) {
    return {
      kind: 'not-found',
      message: 'The requested appointment or related record was not found.',
    };
  }
  if (error.status === 400) {
    return { kind: 'validation', message: 'Check the appointment information and try again.' };
  }
  if (error.status === 409) {
    if (detail.includes('overlapping appointment')) {
      return {
        kind: 'overlap',
        message:
          'The patient already has an overlapping appointment. Change the time and try again.',
      };
    }
    if (detail.includes('appointment reference') && detail.includes('already exists')) {
      return {
        kind: 'duplicate-reference',
        message: 'An appointment with this reference already exists.',
      };
    }
    if (detail.includes('cannot be scheduled')) {
      return {
        kind: 'referral-state',
        message: 'This referral is no longer in a state that permits appointment scheduling.',
      };
    }
    if (title === 'invalid state transition') {
      return {
        kind: 'workflow',
        message:
          'The appointment changed and this action is no longer valid. Reload the appointment.',
      };
    }
    return {
      kind: 'conflict',
      message: 'The appointment could not be changed because of a conflict.',
    };
  }

  return { kind: 'generic', message: fallback };
}
