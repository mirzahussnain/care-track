import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Surface } from './surface';

describe('Surface', () => {
  let fixture: ComponentFixture<Surface>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Surface] }).compileComponents();
    fixture = TestBed.createComponent(Surface);
    fixture.detectChanges();
  });

  it('creates with the default variant and medium padding', () => {
    const surface = fixture.nativeElement.querySelector('.ct-surface') as HTMLElement;
    expect(fixture.componentInstance).toBeTruthy();
    expect(surface.classList.contains('ct-surface--default')).toBe(true);
    expect(surface.classList.contains('ct-surface--padding-md')).toBe(true);
  });

  it('applies the requested variant', () => {
    fixture.componentRef.setInput('variant', 'elevated');
    fixture.detectChanges();
    const surface = fixture.nativeElement.querySelector('.ct-surface') as HTMLElement;
    expect(surface.classList.contains('ct-surface--elevated')).toBe(true);
  });

  it('applies the requested padding', () => {
    fixture.componentRef.setInput('padding', 'none');
    fixture.detectChanges();
    const surface = fixture.nativeElement.querySelector('.ct-surface') as HTMLElement;
    expect(surface.classList.contains('ct-surface--padding-none')).toBe(true);
  });
});
