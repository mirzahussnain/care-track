import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { Button, EmptyState, FormField, Surface } from '../../../../design-system/components';
import { PageHeader } from '../../../../design-system/patterns';
import { PatientIdentityBanner } from '../../../../design-system/patterns/patient-identity-banner/patient-identity-banner';
import { ReferralPatientSummary } from '../../../patients/models/patient.models';
import { ReferralPatientSelector } from '../../components/referral-patient-selector/referral-patient-selector';
import { ReferralApiService } from '../../data-access/referral-api.service';
import { referralErrorMessage } from '../../models/referral-errors';
import {
  REFERRAL_PRIORITIES,
  ReferralPriority,
  referralPriorityLabel,
} from '../../models/referral.models';

@Component({
  selector: 'app-create-referral-page',
  standalone: true,
  imports: [
    Button,
    EmptyState,
    FormField,
    PageHeader,
    PatientIdentityBanner,
    ReactiveFormsModule,
    ReferralPatientSelector,
    RouterLink,
    Surface,
  ],
  templateUrl: './create-referral-page.html',
  styleUrl: './create-referral-page.css',
})
export class CreateReferralPage {
  private readonly referralApi = inject(ReferralApiService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly authService = inject(AuthService);

  readonly priorities = Object.values(REFERRAL_PRIORITIES) as ReferralPriority[];
  readonly canManageReferrals = computed(
    () =>
      this.authService.hasRole(CARETRACK_ROLES.clinician) ||
      this.authService.hasRole(CARETRACK_ROLES.referralCoordinator),
  );
  readonly selectedPatient = signal<ReferralPatientSummary | null>(null);
  readonly submitted = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  readonly form = new FormGroup({
    patientId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required],
    }),
    referralReference: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(30)],
    }),
    priority: new FormControl<ReferralPriority>(REFERRAL_PRIORITIES.routine, {
      nonNullable: true,
      validators: [Validators.required],
    }),
    reason: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(2000)],
    }),
  });

  selectPatient(patient: ReferralPatientSummary): void {
    this.selectedPatient.set(patient);
    this.form.controls.patientId.setValue(patient.id);
    this.form.controls.patientId.markAsTouched();
  }

  submit(): void {
    this.submitted.set(true);
    this.errorMessage.set(null);
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);

    this.referralApi
      .createReferral({
        patientId: value.patientId,
        referralReference: value.referralReference.trim(),
        priority: value.priority,
        reason: value.reason.trim(),
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (referral) => {
          this.submitting.set(false);
          void this.router.navigate(['/referrals', referral.id]);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          const mapped = referralErrorMessage(
            error,
            'The referral could not be created. Try again.',
          );
          this.errorMessage.set(
            mapped.kind === 'conflict'
              ? 'A referral with this reference already exists.'
              : mapped.kind === 'not-found'
                ? 'The selected patient no longer exists. Search for the patient again.'
                : mapped.message,
          );
        },
      });
  }

  priorityLabel(priority: ReferralPriority): string {
    return referralPriorityLabel(priority);
  }

  errorFor(
    control: FormControl<string>,
    label: string,
    maximum?: number,
  ): string | undefined {
    if (!(control.touched || this.submitted())) {
      return undefined;
    }
    if (control.hasError('required') || !control.value.trim()) {
      return `${label} is required.`;
    }
    if (maximum && control.hasError('maxlength')) {
      return `${label} cannot exceed ${maximum} characters.`;
    }
    return undefined;
  }
}
