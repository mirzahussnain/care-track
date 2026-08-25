import {
  Component,
  signal,
} from '@angular/core';

import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';

import {
  AuthService,
} from './auth.service';

import {
  CARETRACK_ROLES,
  CareTrackRole,
} from './auth.models';

import {
  HasRoleDirective,
} from './has-role.directive';

@Component({
  imports: [
    HasRoleDirective,
  ],
  template: `
    <p
      *ctHasRole="
        roles.clinician
      "
    >
      Clinician content
    </p>

    <p
      *ctHasRole="[
        roles.clinician,
        roles.referralCoordinator
      ]"
    >
      Shared content
    </p>
  `,
})
class TestHostComponent {
  readonly roles =
    CARETRACK_ROLES;
}

describe(
  'HasRoleDirective',
  () => {
    let fixture:
      ComponentFixture<
        TestHostComponent
      >;

    const rolesSignal =
      signal<
        readonly CareTrackRole[]
      >([]);

    const authServiceMock = {
      hasRole: (
        role: CareTrackRole
      ) =>
        rolesSignal()
          .includes(role),
    };

    beforeEach(async () => {
      rolesSignal.set([]);

      await TestBed
        .configureTestingModule({
          imports: [
            TestHostComponent,
          ],

          providers: [
            {
              provide:
                AuthService,

              useValue:
                authServiceMock,
            },
          ],
        })
        .compileComponents();

      fixture =
        TestBed.createComponent(
          TestHostComponent
        );

      fixture.detectChanges();
    });

    it(
      'hides role-protected content when the user has no matching role',
      () => {
        const text =
          fixture.nativeElement
            .textContent;

        expect(text)
          .not.toContain(
            'Clinician content'
          );

        expect(text)
          .not.toContain(
            'Shared content'
          );
      }
    );

    it(
      'shows clinician-only content for a clinician',
      () => {
        rolesSignal.set([
          CARETRACK_ROLES
            .clinician,
        ]);

        fixture.detectChanges();

        const text =
          fixture.nativeElement
            .textContent;

        expect(text)
          .toContain(
            'Clinician content'
          );

        expect(text)
          .toContain(
            'Shared content'
          );
      }
    );

    it(
      'shows shared content but not clinician-only content for a referral coordinator',
      () => {
        rolesSignal.set([
          CARETRACK_ROLES
            .referralCoordinator,
        ]);

        fixture.detectChanges();

        const text =
          fixture.nativeElement
            .textContent;

        expect(text)
          .not.toContain(
            'Clinician content'
          );

        expect(text)
          .toContain(
            'Shared content'
          );
      }
    );

    it(
      'reacts when roles change',
      () => {
        let text =
          fixture.nativeElement
            .textContent;

        expect(text)
          .not.toContain(
            'Clinician content'
          );

        rolesSignal.set([
          CARETRACK_ROLES
            .clinician,
        ]);

        fixture.detectChanges();

        text =
          fixture.nativeElement
            .textContent;

        expect(text)
          .toContain(
            'Clinician content'
          );
      }
    );
  }
);