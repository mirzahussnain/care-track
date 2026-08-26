import { DatePipe } from '@angular/common';
import { Component, effect, input, output, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import {
  Button,
  EmptyState,
  FormField,
  Skeleton,
  Surface,
} from '../../../../design-system/components';
import { ClinicalNote } from '../../models/clinical-note.models';

export interface UpdateClinicalNoteEvent {
  readonly id: string;
  readonly content: string;
}

@Component({
  selector: 'app-clinical-notes-section',
  standalone: true,
  imports: [Button, DatePipe, EmptyState, FormField, ReactiveFormsModule, Skeleton, Surface],
  templateUrl: './clinical-notes-section.html',
  styleUrl: './clinical-notes-section.css',
})
export class ClinicalNotesSection {
  readonly notes = input.required<readonly ClinicalNote[]>();
  readonly loading = input(false);
  readonly loadError = input<string | null>(null);
  readonly mutationError = input<string | null>(null);
  readonly creating = input(false);
  readonly updatingId = input<string | null>(null);
  readonly currentUserId = input('');
  readonly currentUserName = input('');
  readonly saveVersion = input(0);

  readonly retryRequested = output<void>();
  readonly createRequested = output<string>();
  readonly updateRequested = output<UpdateClinicalNoteEvent>();

  readonly submitted = signal(false);
  readonly editingId = signal<string | null>(null);
  readonly editSubmitted = signal(false);

  readonly createForm = new FormGroup({
    content: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(5000)],
    }),
  });
  readonly editForm = new FormGroup({
    content: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(5000)],
    }),
  });

  private observedSaveVersion = 0;

  constructor() {
    effect(() => {
      const version = this.saveVersion();
      if (version <= this.observedSaveVersion) return;
      this.observedSaveVersion = version;
      this.createForm.reset({ content: '' });
      this.submitted.set(false);
      this.cancelEdit();
    });
  }

  createNote(): void {
    this.submitted.set(true);
    if (this.createForm.invalid || this.creating()) {
      this.createForm.markAllAsTouched();
      return;
    }
    this.createRequested.emit(this.createForm.controls.content.value.trim());
  }

  beginEdit(note: ClinicalNote): void {
    this.editingId.set(note.id);
    this.editSubmitted.set(false);
    this.editForm.reset({ content: note.content });
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editSubmitted.set(false);
    this.editForm.reset({ content: '' });
  }

  updateNote(id: string): void {
    this.editSubmitted.set(true);
    if (this.editForm.invalid || this.updatingId()) {
      this.editForm.markAllAsTouched();
      return;
    }
    this.updateRequested.emit({ id, content: this.editForm.controls.content.value.trim() });
  }

  contentError(control: FormControl<string>, submitted: boolean): string | undefined {
    if (!(control.touched || submitted)) return undefined;
    if (control.hasError('required') || !control.value.trim())
      return 'Clinical note content is required.';
    return control.hasError('maxlength')
      ? 'Clinical note content cannot exceed 5,000 characters.'
      : undefined;
  }

  authorLabel(note: ClinicalNote): string {
    if (note.createdBy === this.currentUserId() && this.currentUserName()) {
      return this.currentUserName();
    }
    return `Author ID: ${note.createdBy}`;
  }
}
