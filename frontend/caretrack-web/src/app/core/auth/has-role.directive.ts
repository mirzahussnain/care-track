import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';

import { AuthService } from './auth.service';

import type { CareTrackRole } from './auth.models';

@Directive({
  selector: '[ctHasRole]',
})
export class HasRoleDirective {
  private readonly authService = inject(AuthService);

  private readonly templateRef = inject(TemplateRef<unknown>);

  private readonly viewContainer = inject(ViewContainerRef);

  readonly ctHasRole = input.required<CareTrackRole | readonly CareTrackRole[]>();

  private hasView = false;

  constructor() {
    effect(() => {
      const requiredRoles = this.normalizeRoles(this.ctHasRole());

      const allowed = requiredRoles.some((role) => this.authService.hasRole(role));

      if (allowed && !this.hasView) {
        this.viewContainer.createEmbeddedView(this.templateRef);

        this.hasView = true;

        return;
      }

      if (!allowed && this.hasView) {
        this.viewContainer.clear();

        this.hasView = false;
      }
    });
  }

  private normalizeRoles(
    roles: CareTrackRole | readonly CareTrackRole[],
  ): readonly CareTrackRole[] {
    return typeof roles === 'string' ? [roles] : roles;
  }
}
