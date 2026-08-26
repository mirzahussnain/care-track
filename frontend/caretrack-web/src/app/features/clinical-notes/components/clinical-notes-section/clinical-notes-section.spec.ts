import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ClinicalNote } from '../../models/clinical-note.models';
import { ClinicalNotesSection } from './clinical-notes-section';

describe('ClinicalNotesSection', () => {
  let fixture: ComponentFixture<ClinicalNotesSection>;
  const currentUserId = '11111111-1111-1111-1111-111111111111';
  const notes: readonly ClinicalNote[] = [
    {
      id: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
      appointmentId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      content: 'First synthetic note.',
      createdBy: currentUserId,
      createdAt: '2026-08-25T10:00:00Z',
      updatedAt: null,
    },
    {
      id: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
      appointmentId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
      content: 'Second synthetic note.',
      createdBy: '22222222-2222-2222-2222-222222222222',
      createdAt: '2026-08-25T11:00:00Z',
      updatedAt: '2026-08-25T11:30:00Z',
    },
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [ClinicalNotesSection] }).compileComponents();
    fixture = TestBed.createComponent(ClinicalNotesSection);
    fixture.componentRef.setInput('notes', notes);
    fixture.componentRef.setInput('currentUserId', currentUserId);
    fixture.componentRef.setInput('currentUserName', 'Dr Amina Khan');
  });

  it('renders notes chronologically with honest author labels and timestamps', () => {
    fixture.detectChanges();
    const text = fixture.nativeElement.textContent as string;

    expect(text.indexOf('First synthetic note.')).toBeLessThan(
      text.indexOf('Second synthetic note.'),
    );
    expect(text).toContain('Dr Amina Khan');
    expect(text).toContain('Author ID: 22222222-2222-2222-2222-222222222222');
    expect(text).toContain('Updated');
  });

  it('validates add content and emits trimmed content only', () => {
    const emit = vi.spyOn(fixture.componentInstance.createRequested, 'emit');
    fixture.componentInstance.createNote();
    expect(emit).not.toHaveBeenCalled();

    fixture.componentInstance.createForm.setValue({ content: ' New synthetic note. ' });
    fixture.componentInstance.createNote();
    expect(emit).toHaveBeenCalledWith('New synthetic note.');
  });

  it('supports inline edit and preserves it until a successful save version arrives', () => {
    const emit = vi.spyOn(fixture.componentInstance.updateRequested, 'emit');
    fixture.componentInstance.beginEdit(notes[0]);
    fixture.componentInstance.editForm.setValue({ content: ' Updated content. ' });
    fixture.componentInstance.updateNote(notes[0].id);

    expect(emit).toHaveBeenCalledWith({ id: notes[0].id, content: 'Updated content.' });
    expect(fixture.componentInstance.editingId()).toBe(notes[0].id);

    fixture.componentRef.setInput('mutationError', 'Update failed.');
    fixture.detectChanges();
    expect(fixture.componentInstance.editingId()).toBe(notes[0].id);

    fixture.componentRef.setInput('saveVersion', 1);
    fixture.detectChanges();
    expect(fixture.componentInstance.editingId()).toBeNull();
  });

  it('shows loading, empty, and independent retryable error states', () => {
    fixture.componentRef.setInput('loading', true);
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('[aria-label="Loading clinical notes"]'),
    ).not.toBeNull();

    fixture.componentRef.setInput('loading', false);
    fixture.componentRef.setInput('notes', []);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('No clinical notes recorded');

    fixture.componentRef.setInput('loadError', 'Clinical notes could not be loaded.');
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Try again');
    expect(fixture.nativeElement.querySelector('.clinical-notes')?.hasAttribute('aria-live')).toBe(
      false,
    );

    fixture.componentRef.setInput('loadError', null);
    fixture.componentRef.setInput('mutationError', 'The clinical note could not be added.');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[role="alert"]')?.textContent).toContain(
      'could not be added',
    );
  });

  it('has no delete control', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).not.toContain('Delete');
    expect(fixture.nativeElement.querySelector('[class*="trash"]')).toBeNull();
  });

  it('associates a non-live character count and visible validation with the add textarea', async () => {
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const textarea = element.querySelector<HTMLTextAreaElement>('#clinical-note-content')!;
    const count = element.querySelector('#clinical-note-content-count')!;

    expect(textarea.required).toBe(true);
    expect(textarea.getAttribute('aria-describedby')).toContain('clinical-note-content-count');
    expect(count.textContent).toContain('0 of 5,000 characters');
    expect(count.hasAttribute('aria-live')).toBe(false);

    fixture.componentInstance.createNote();
    fixture.detectChanges();
    await Promise.resolve();

    expect(textarea.getAttribute('aria-invalid')).toBe('true');
    expect(textarea.getAttribute('aria-describedby')).toContain('clinical-note-content-error');
    expect(element.querySelector('#clinical-note-content-error')).not.toBeNull();
    expect(document.activeElement).toBe(textarea);
  });

  it('uses ordered articles with author and time semantics without live note content', () => {
    fixture.detectChanges();
    const element = fixture.nativeElement as HTMLElement;
    const list = element.querySelector('ol.clinical-notes__list');
    const articles = list?.querySelectorAll('article') ?? [];

    expect(list).not.toBeNull();
    expect(articles).toHaveLength(notes.length);
    expect(articles[0].querySelector('time')?.getAttribute('datetime')).toBe(notes[0].createdAt);
    expect(articles[0].querySelector('.clinical-notes__content')?.hasAttribute('aria-live')).toBe(
      false,
    );
  });
});
