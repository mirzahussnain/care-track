import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

@Component({
  selector: 'ct-page-header',
  standalone: true,
  templateUrl: './page-header.html',
  styleUrl: './page-header.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeader {
  readonly title = input.required<string>();
  readonly description = input<string>();
}
