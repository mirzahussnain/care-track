import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Skeleton } from './skeleton';

describe('Skeleton', () => {
  let fixture: ComponentFixture<Skeleton>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Skeleton] }).compileComponents();
    fixture = TestBed.createComponent(Skeleton);
    fixture.detectChanges();
  });

  it('creates with the text variant by default', () => {
    const skeleton = fixture.nativeElement.querySelector('.ct-skeleton') as HTMLElement;
    expect(fixture.componentInstance).toBeTruthy();
    expect(skeleton.classList.contains('ct-skeleton--text')).toBe(true);
  });

  it('applies the requested variant', () => {
    fixture.componentRef.setInput('variant', 'circle');
    fixture.detectChanges();
    const skeleton = fixture.nativeElement.querySelector('.ct-skeleton') as HTMLElement;
    expect(skeleton.classList.contains('ct-skeleton--circle')).toBe(true);
  });

  it('is hidden from assistive technology', () => {
    expect(fixture.nativeElement.getAttribute('aria-hidden')).toBe('true');
  });
});
