import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const trimmedRequired: ValidatorFn = (control: AbstractControl): ValidationErrors | null =>
  control.value.trim() ? null : { required: true };

export const dateNotInFuture: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  if (!control.value) {
    return null;
  }

  const today = new Date().toISOString().slice(0, 10);
  return String(control.value) > today ? { futureDate: true } : null;
};

export function fieldError(
  control: AbstractControl,
  label: string,
  maxLength?: number,
): string | undefined {
  if (control.hasError('required')) {
    return `${label} is required.`;
  }
  if (control.hasError('maxlength') && maxLength) {
    return `${label} must be ${maxLength} characters or fewer.`;
  }
  if (control.hasError('futureDate')) {
    return 'Date of birth cannot be in the future.';
  }
  return undefined;
}
