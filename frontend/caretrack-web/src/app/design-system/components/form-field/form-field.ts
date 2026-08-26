import { ChangeDetectionStrategy, Component, input } from '@angular/core';

@Component({
  selector: 'ct-form-field',
  standalone: true,
  templateUrl: './form-field.html',
  styleUrl: './form-field.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormField {
  readonly label = input.required<string>();
  readonly forId = input.required<string>();
  readonly hint = input<string>();
  readonly error = input<string>();
  readonly required = input(false);
}
