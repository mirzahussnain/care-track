import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';

export type IconButtonVariant =
  | 'default'
  | 'ghost'
  | 'danger';

export type IconButtonSize =
  | 'sm'
  | 'md'
  | 'lg';

@Component({
  selector: 'ct-icon-button',
  standalone: true,
  templateUrl: './icon-button.html',
  styleUrl: './icon-button.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class IconButton {
  readonly ariaLabel = input.required<string>();

  readonly variant =
    input<IconButtonVariant>('default');

  readonly size =
    input<IconButtonSize>('md');

  readonly type =
    input<'button' | 'submit' | 'reset'>('button');

  readonly disabled = input(false);
}