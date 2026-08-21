import { Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import type { ShellNavigationItem } from '../shell-navigation';

@Component({
  selector: 'app-sidebar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './app-sidebar.html',
  styleUrl: './app-sidebar.css',
})
export class AppSidebar {
  readonly navigation = input.required<readonly ShellNavigationItem[]>();
  readonly showBrand = input(true);
  readonly showCollapseControl = input(true);
  readonly collapsed=input(false);
  readonly collapseToggle=output<void>();
  readonly navigationSelected = output<void>();
}