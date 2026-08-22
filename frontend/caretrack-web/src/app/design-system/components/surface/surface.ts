import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

export type SurfaceVariant = 'default' | 'subtle' | 'elevated';
export type SurfacePadding = 'none' | 'sm' | 'md' | 'lg';

@Component({
  selector: 'ct-surface',
  standalone: true,
  templateUrl: './surface.html',
  styleUrl: './surface.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Surface {
  readonly variant = input<SurfaceVariant>('default');
  readonly padding = input<SurfacePadding>('md');
}
