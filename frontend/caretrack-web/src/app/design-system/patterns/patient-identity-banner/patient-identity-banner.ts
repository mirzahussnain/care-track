import { DatePipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'ct-patient-identity-banner',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './patient-identity-banner.html',
  styleUrl: './patient-identity-banner.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PatientIdentityBanner {
  readonly fullName = input.required<string>();
  readonly patientReference = input.required<string>();
  readonly dateOfBirth = input.required<string>();
}
