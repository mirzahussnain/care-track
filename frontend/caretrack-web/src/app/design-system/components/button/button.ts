import {
  ChangeDetectionStrategy,
  Component,
  input,
} from '@angular/core';


export type ButtonVariant =
  | 'primary'
  | 'secondary'
  | 'ghost'
  | 'danger';

export type ButtonSize =
  | 'sm'
  | 'md'
  | 'lg';

  @Component({
    selector:'ct-button',
    standalone:true,
    templateUrl:'./button.html',
    styleUrl:'./button.css',
    changeDetection:ChangeDetectionStrategy.OnPush,
  })
  export class Button{
     readonly variant = input<ButtonVariant>('primary');
  readonly size = input<ButtonSize>('md');
   readonly type = input<'button' | 'submit' | 'reset'>('button');
     readonly disabled = input(false);
  readonly loading = input(false);

  }