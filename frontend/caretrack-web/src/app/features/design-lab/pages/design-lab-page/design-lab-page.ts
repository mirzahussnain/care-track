import { Component, signal } from '@angular/core';
import { EditorialOperationsComponent } from '../../components/editorial-operations/editorial-operations';
import { QuietClinicalComponent } from '../../components/quiet-clinical/quiet-clinical';
import { StructuredModernComponent } from '../../components/structured-modern/structured-modern';

import {
  Button,
  DataToolbar,
  EmptyState,
  FormField,
  IconButton,
  PageHeader,
  Skeleton,
  StatusChip,
  Surface,
} from '../../../../design-system';

type DesignDirection = 'quiet' | 'editorial' | 'structured';

@Component({
  selector: 'app-design-lab-page',
  imports: [
    QuietClinicalComponent,
    EditorialOperationsComponent,
    StructuredModernComponent,
    Button,
    DataToolbar,
    EmptyState,
    FormField,
    IconButton,
    PageHeader,
    Skeleton,
    StatusChip,
    Surface,
  ],
  templateUrl: './design-lab-page.html',
  styleUrl: './design-lab-page.css',
})
export class DesignLabPage {
  readonly activeDirection = signal<DesignDirection>('quiet');

  selectDirection(direction: DesignDirection): void {
    this.activeDirection.set(direction);
  }
}
