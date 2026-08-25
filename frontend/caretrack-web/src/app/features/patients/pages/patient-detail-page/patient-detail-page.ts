import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, RouterLink } from '@angular/router';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { HasRoleDirective } from '../../../../core/auth/has-role.directive';
import { Button, EmptyState, Skeleton, Surface } from '../../../../design-system/components';
import { PageHeader } from '../../../../design-system/patterns';
import { PatientIdentityBanner } from '../../../../design-system/patterns/patient-identity-banner/patient-identity-banner';
import { PatientApiService } from '../../data-access/patient-api.service';
import { Patient } from '../../models/patient.models';

type DetailError = 'not-found' | 'forbidden' | 'generic' | null;

@Component({
  selector: 'app-patient-detail-page',
  standalone: true,
  imports: [
    Button,
    DatePipe,
    EmptyState,
    HasRoleDirective,
    PageHeader,
    PatientIdentityBanner,
    RouterLink,
    Skeleton,
    Surface,
  ],
  templateUrl: './patient-detail-page.html',
  styleUrl: './patient-detail-page.css',
})
export class PatientDetailPage {
  private readonly patientApi = inject(PatientApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly clinicianRole = CARETRACK_ROLES.clinician;
  readonly patientManagers = [
    CARETRACK_ROLES.clinician,
    CARETRACK_ROLES.referralCoordinator,
  ] as const;
  readonly patientId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly patient = signal<Patient | null>(null);
  readonly loading = signal(true);
  readonly error = signal<DetailError>(null);

  constructor() {
    this.loadPatient();
  }

  retry(): void {
    this.loadPatient();
  }

  private loadPatient(): void {
    this.loading.set(true);
    this.error.set(null);
    this.patientApi
      .getPatient(this.patientId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (patient) => {
          this.patient.set(patient);
          this.loading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          this.patient.set(null);
          this.error.set(
            error.status === 404 ? 'not-found' : error.status === 403 ? 'forbidden' : 'generic',
          );
          this.loading.set(false);
        },
      });
  }
}
