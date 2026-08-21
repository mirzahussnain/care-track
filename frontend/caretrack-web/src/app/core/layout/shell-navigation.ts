export interface ShellNavigationItem {
  label: string;
  route: string;
  icon: string;
  exact: boolean;
}

export const SHELL_NAVIGATION: readonly ShellNavigationItem[] = [
  {
    label: 'Dashboard',
    route: '/dashboard',
    icon: 'ph-squares-four',
    exact: true,
  },
];