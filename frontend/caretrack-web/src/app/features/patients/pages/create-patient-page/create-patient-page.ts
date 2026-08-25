import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

import { CARETRACK_ROLES } from '../../../../core/auth/auth.models';
import { AuthService } from '../../../../core/auth/auth.service';
import { Button, FormField, Surface } from '../../../../design-system/components';
import { PageHeader } from '../../../../design-system/patterns';
import { PatientIdentityBanner } from '../../../../design-system/patterns/patient-identity-banner/patient-identity-banner';
import { PatientApiService } from '../../data-access/patient-api.service';
import { dateNotInFuture, fieldError, trimmedRequired } from '../../forms/patient-form.validators';
import { Patient } from '../../models/patient.models';

type CreateError = 'validation' | 'duplicate' | 'forbidden' | 'generic' | null;

@Component({
  selector: 'app-create-patient-page',
  standalone: true,
  imports: [
    Button,
    FormField,
    PageHeader,
    PatientIdentityBanner,
    ReactiveFormsModule,
    RouterLink,
    Surface,
  ],
  templateUrl: './create-patient-page.html',
  styleUrl: './create-patient-page.css',
})
export class CreatePatientPage {
  private readonly patientApi = inject(PatientApiService);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  readonly maxDateOfBirth = new Date().toISOString().slice(0, 10);
  readonly submitting = signal(false);
  readonly submitted = signal(false);
  readonly error = signal<CreateError>(null);
  readonly createdPatient = signal<Patient | null>(null);

  readonly form = new FormGroup({
    patientReference: new FormControl('', {
      nonNullable: true,
      validators: [trimmedRequired, Validators.maxLength(20)],
    }),
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

  submit(): void {
    this.submitted.set(true);
    this.error.set(null);
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    this.submitting.set(true);
    this.patientApi
      .createPatient({
        patientReference: value.patientReference.trim(),
        firstName: value.firstName.trim(),
        lastName: value.lastName.trim(),
        dateOfBirth: value.dateOfBirth,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (patient) => {
          this.submitting.set(false);
          if (this.authService.hasRole(CARETRACK_ROLES.clinician)) {
            void this.router.navigate(['/patients', patient.id]);
            return;
          }
          this.createdPatient.set(patient);
        },
        error: (error: HttpErrorResponse) => {
          this.submitting.set(false);
          this.error.set(
            error.status === 409
              ? 'duplicate'
              : error.status === 400
                ? 'validation'
                : error.status === 403
                  ? 'forbidden'
                  : 'generic',
          );
        },
      });
  }

  registerAnother(): void {
    this.createdPatient.set(null);
    this.error.set(null);
    this.submitted.set(false);
    this.form.reset();
  }

  errorFor(control: FormControl<string>, label: string, maxLength?: number): string | undefined {
    return control.touched || this.submitted() ? fieldError(control, label, maxLength) : undefined;
  }
}
