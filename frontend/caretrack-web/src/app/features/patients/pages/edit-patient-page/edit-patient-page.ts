import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { HasRoleDirective } from '../../../../core/auth/has-role.directive';
import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  Surface,
} from '../../../../design-system/components';
import { PageHeader } from '../../../../design-system/patterns';
import { PatientIdentityBanner } from '../../../../design-system/patterns/patient-identity-banner/patient-identity-banner';
import { PatientApiService } from '../../data-access/patient-api.service';
import { dateNotInFuture, fieldError, trimmedRequired } from '../../forms/patient-form.validators';
import { Patient } from '../../models/patient.models';
import { focusFirstInvalidControl } from '../../../../shared/utils/focus-management';

type EditLoadError = 'not-found' | 'forbidden' | 'generic' | null;
type EditSubmitError = 'validation' | 'forbidden' | 'not-found' | 'generic' | null;

@Component({
  selector: 'app-edit-patient-page',
  standalone: true,
  imports: [
    Button,
    EmptyState,
    FormField,
    HasRoleDirective,
    PageHeader,
    PatientIdentityBanner,
    ReactiveFormsModule,
    RouterLink,
    Skeleton,
    Surface,
  ],
  templateUrl: './edit-patient-page.html',
  styleUrl: './edit-patient-page.css',
})
export class EditPatientPage {
  private readonly patientApi = inject(PatientApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private loadSubscription?: Subscription;

  readonly patientManagers = [
    CARETRACK_ROLES.clinician,
    CARETRACK_ROLES.referralCoordinator,
  ] as const;
  readonly patientId = this.route.snapshot.paramMap.get('id') ?? '';
  readonly maxDateOfBirth = new Date().toISOString().slice(0, 10);
  readonly patient = signal<Patient | null>(null);
  readonly originalRowVersion = signal<string | null>(null);
  readonly loading = signal(true);
  readonly loadError = signal<EditLoadError>(null);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly submitError = signal<EditSubmitError>(null);
  readonly conflict = signal(false);
  readonly reloading = signal(false);
  readonly reloadError = signal(false);

  readonly form = new FormGroup({
    firstName: new FormControl('', {
      nonNullable: true,
      validators: [trimmedRequired, Validators.maxLength(100)],
    }),
    lastName: new FormControl('', {
      nonNullable: true,
      validators: [trimmedRequired, Validators.maxLength(100)],
    }),
    dateOfBirth: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, dateNotInFuture],
    }),
  });

  constructor() {
    this.destroyRef.onDestroy(() => this.loadSubscription?.unsubscribe());
    this.loadPatient(false);
  }

  submit(): void {
    this.submitted.set(true);
    this.submitError.set(null);
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      focusFirstInvalidControl(['edit-first-name', 'edit-last-name', 'edit-date-of-birth']);
      return;
    }
    if (this.submitting() || this.conflict()) return;

    const rowVersion = this.originalRowVersion();
    if (!rowVersion) {
      this.submitError.set('generic');
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.patientApi
      .updatePatient(this.patientId, {
        firstName: value.firstName.trim(),
        lastName: value.lastName.trim(),
        dateOfBirth: value.dateOfBirth,
        rowVersion,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.submitting.set(false);
          void this.router.navigate(['/patients', this.patientId]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          if (error.status === 409) {
            this.conflict.set(true);
            return;
          }
          this.submitError.set(
            error.status === 400
              ? 'validation'
              : error.status === 403
                ? 'forbidden'
                : error.status === 404
                  ? 'not-found'
                  : 'generic',
          );
        },
      });
  }

  reloadLatest(): void {
    if (!this.conflict() || this.reloading()) {
      return;
    }
    this.loadPatient(true);
  }

  retryInitialLoad(): void {
    this.loadPatient(false);
  }

  errorFor(control: FormControl<string>, label: string, maxLength?: number): string | undefined {
    return control.touched || this.submitted() ? fieldError(control, label, maxLength) : undefined;
  }

  private loadPatient(isReload: boolean): void {
    this.loadSubscription?.unsubscribe();
    if (isReload) {
      this.reloading.set(true);
      this.reloadError.set(false);
    } else {
      this.loading.set(true);
      this.loadError.set(null);
    }

    this.loadSubscription = this.patientApi
      .getPatient(this.patientId)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (patient) => {
          this.patient.set(patient);
          this.originalRowVersion.set(patient.rowVersion);
          this.form.setValue({
            firstName: patient.firstName,
            lastName: patient.lastName,
            dateOfBirth: patient.dateOfBirth,
          });
          this.form.markAsPristine();
          this.form.markAsUntouched();
          this.submitted.set(false);
          this.submitError.set(null);
          this.conflict.set(false);
          this.loading.set(false);
          this.reloading.set(false);
        },
        error: (error: HttpErrorResponse) => {
          if (isReload) {
            this.reloadError.set(true);
            this.reloading.set(false);
            return;
          }
          this.patient.set(null);
          this.originalRowVersion.set(null);
          this.loadError.set(
            error.status === 404 ? 'not-found' : error.status === 403 ? 'forbidden' : 'generic',
          );
          this.loading.set(false);
        },
      });
  }
}
