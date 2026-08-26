import { Component, computed, signal, inject, HostListener } from '@angular/core';
import { RouterOutlet, ActivatedRoute, NavigationEnd, Router } from '@angular/router';
import { AppSidebar } from '../app-sidebar/app-sidebar';
import { AppTopbar } from '../app-topbar/app-topbar';
import { SHELL_NAVIGATION } from '../shell-navigation';
import { toSignal } from '@angular/core/rxjs-interop';
import { filter, map } from 'rxjs';

import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, AppSidebar, AppTopbar],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.css',
})
export class AppShell {
  readonly navigation = SHELL_NAVIGATION;
  readonly sidebarCollapsed = signal(false);
  readonly mobileNavigationOpen = signal(false);
  private readonly router = inject(Router);
  private readonly activatedRoute = inject(ActivatedRoute);
  private readonly authService = inject(AuthService);

  readonly accountName = computed(() => {
    const user = this.authService.currentUser();
    return user?.name.trim() || user?.username.trim() || 'Signed-in user';
  });

  readonly accountRoleLabel = computed(() => {
    const labels: Record<string, string> = {
      Clinician: 'Clinician',
      ReferralCoordinator: 'Referral Coordinator',
      Administrator: 'Administrator',
    };

    const roles = this.authService.roles();
    return roles.length > 0
      ? roles.map((role) => labels[role] ?? role).join(' / ')
      : 'Authenticated user';
  });

  readonly areaLabel = toSignal(
    this.router.events.pipe(
      filter((event): event is NavigationEnd => event instanceof NavigationEnd),

      map(() => this.getActiveAreaLabel()),
    ),
    {
      initialValue: 'Dashboard',
    },
  );

  toggleSidebar(): void {
    this.sidebarCollapsed.update((collapsed) => !collapsed);
  }

  openMobileNavigation(): void {
    this.mobileNavigationOpen.set(true);
  }

  closeMobileNavigation(): void {
    this.mobileNavigationOpen.set(false);
  }

  signOut(): void {
    this.closeMobileNavigation();
    this.authService.signOut();
  }

  private getActiveAreaLabel(): string {
    let route = this.activatedRoute;

    while (route.firstChild) {
      route = route.firstChild;
    }

    return route.snapshot?.data?.['areaLabel'] ?? 'CareTrack';
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.mobileNavigationOpen()) {
      this.closeMobileNavigation();
    }
  }
}
