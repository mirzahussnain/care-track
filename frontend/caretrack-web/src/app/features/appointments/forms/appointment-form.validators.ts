import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

import { appointmentInputTimestamp } from '../models/appointment-datetime';

export function appointmentUtcRangeValidator(
  startControlName: string,
  endControlName: string,
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const startValue = control.get(startControlName)?.value as string | undefined;
    const endValue = control.get(endControlName)?.value as string | undefined;
    if (!startValue || !endValue) {
      return null;
    }

    const start = appointmentInputTimestamp(startValue);
    const end = appointmentInputTimestamp(endValue);
    if (start === null || end === null) {
      return { invalidUtcDateTime: true };
    }

    return end > start ? null : { appointmentRange: true };
  };
}
