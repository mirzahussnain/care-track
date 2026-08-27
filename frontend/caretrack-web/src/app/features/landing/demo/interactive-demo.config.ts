export type InteractiveDemoAccountId = 'referral-coordinator' | 'clinician';

export interface InteractiveDemoAccount {
  readonly id: InteractiveDemoAccountId;
  readonly role: 'ReferralCoordinator' | 'Clinician';
  readonly roleLabel: string;
  readonly icon: string;
  readonly summary: string;
  readonly email: string;
  readonly password: string;
  readonly capabilities: readonly string[];
  readonly boundary: string;
}

export const INTERACTIVE_DEMO_ACCOUNTS: readonly InteractiveDemoAccount[] = [
  {
    id: 'referral-coordinator',
    role: 'ReferralCoordinator',
    roleLabel: 'Referral Coordinator',
    icon: 'ph-arrows-split',
    summary: 'Coordinate intake, triage, assignment, and referral-linked scheduling.',
    email: 'demo-caretrack-rc@devalix01gmail.onmicrosoft.com',
    password: 'CT@referral123',
    capabilities: [
      'Register and update patient records',
      'Create, triage, assign, and progress referrals',
      'Schedule referral-linked appointments',
    ],
    boundary: 'Clinical Notes and clinician appointment actions remain unavailable.',
  },
  {
    id: 'clinician',
    role: 'Clinician',
    roleLabel: 'Clinician',
    icon: 'ph-stethoscope',
    summary: 'Review clinical work, progress referrals, and document completed care.',
    email: 'demo-user-cl@devalix01gmail.onmicrosoft.com',
    password: 'CT@clinician123',
    capabilities: [
      'Search and view patient records',
      'Manage the permitted referral workflow',
      'Schedule and progress appointments',
      'Create and update Clinical Notes',
    ],
    boundary: 'Access remains limited to the Clinician role and API policies.',
  },
];
