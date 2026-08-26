import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'ct-data-toolbar',
  standalone: true,
  templateUrl: './data-toolbar.html',
  styleUrl: './data-toolbar.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DataToolbar {}
