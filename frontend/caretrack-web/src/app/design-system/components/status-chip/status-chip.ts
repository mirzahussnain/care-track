import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import type { SemanticTone } from '../../tokens';

@Component({
  selector: 'ct-status-chip',
  standalone: true,
  templateUrl: './status-chip.html',
  styleUrl: './status-chip.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatusChip {
  readonly tone = input<SemanticTone>('neutral');
}
