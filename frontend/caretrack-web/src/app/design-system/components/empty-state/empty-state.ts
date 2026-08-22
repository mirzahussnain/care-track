import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

@Component({
  selector: 'ct-empty-state',
  standalone: true,
  templateUrl: './empty-state.html',
  styleUrl: './empty-state.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class EmptyState {
  readonly title = input.required<string>();
  readonly description = input<string>();
  readonly icon = input<string>();
}
