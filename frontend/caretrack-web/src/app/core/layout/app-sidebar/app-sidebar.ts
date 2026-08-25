import { Component, computed, inject, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import type { ShellNavigationItem } from '../shell-navigation';
import { AuthService } from '../../auth/auth.service';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-sidebar.html',
  styleUrl: './app-sidebar.css',
})
export class AppSidebar {
private readonly authService = inject(AuthService);
  readonly navigation = input.required<readonly ShellNavigationItem[]>();
  readonly showBrand = input(true);
  readonly showCollapseControl = input(true);
  readonly collapsed=input(false);
  readonly collapseToggle=output<void>();
  readonly navigationSelected = output<void>();
  readonly visibleNavigation =
    computed(() =>
      this.navigation().filter(
        item =>
          !item.roles ||
          item.roles.some(
            role =>
              this.authService.hasRole(
                role
              )
          )
      )
    );
}