export interface AuthenticatedUser {
  readonly id: string;
  readonly name: string;
  readonly username: string;
  readonly roles: readonly string[];
  readonly isDemoAccount: boolean;
}

export type AuthLoadStatus = 'idle' | 'loading' | 'ready' | 'error';

export const CARETRACK_ROLES = {
  clinician: 'Clinician',
  referralCoordinator: 'ReferralCoordinator',
  administrator: 'Administrator',
} as const;

export type CareTrackRole = (typeof CARETRACK_ROLES)[keyof typeof CARETRACK_ROLES];
