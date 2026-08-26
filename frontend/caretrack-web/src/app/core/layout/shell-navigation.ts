import { CARETRACK_ROLES, CareTrackRole } from "../auth/auth.models";

export interface ShellNavigationItem {
  label: string;
  route: string;
  icon: string;
  exact: boolean;
  readonly roles?: readonly CareTrackRole[];
}

export const SHELL_NAVIGATION: readonly ShellNavigationItem[] = [
  {
    label: 'Dashboard',
    route: '/dashboard',
    icon: 'ph-squares-four',
    exact: true,
  },
  {
      label: 'Patients',
      route: '/patients',
      icon: 'ph-users',
      exact: false,
      roles: [
        CARETRACK_ROLES.clinician,
        CARETRACK_ROLES.referralCoordinator,
      ],
    },

    {
      label: 'Referrals',
      route: '/referrals',
      icon: 'ph-files',
      exact: false,
      roles: [
        CARETRACK_ROLES.clinician,
        CARETRACK_ROLES.referralCoordinator,
      ],
    },

    {
      label: 'Appointments',
      route: '/appointments',
      icon: 'ph-calendar-dots',
      exact: false,
      roles: [
        CARETRACK_ROLES.clinician,
      ],
    },
];
