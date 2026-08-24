export interface AuthenticatedUser {
  readonly id: string;
  readonly name: string;
  readonly username: string;
  readonly roles: readonly string[];
}

export const CARETRACK_ROLES = {
  clinician: 'Clinician',
  referralCoordinator: 'ReferralCoordinator',
  administrator: 'Administrator',
} as const;

export type CareTrackRole =
  typeof CARETRACK_ROLES[
    keyof typeof CARETRACK_ROLES
  ];