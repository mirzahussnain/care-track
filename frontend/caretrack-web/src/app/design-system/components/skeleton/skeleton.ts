import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

export type SkeletonVariant = 'text' | 'block' | 'circle';

@Component({
  selector: 'ct-skeleton',
  standalone: true,
  templateUrl: './skeleton.html',
  styleUrl: './skeleton.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    'aria-hidden': 'true',
  },
})
export class Skeleton {
  readonly variant = input<SkeletonVariant>('text');
}
