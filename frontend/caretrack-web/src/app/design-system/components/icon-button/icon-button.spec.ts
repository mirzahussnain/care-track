import { ComponentFixture, TestBed } from '@angular/core/testing';

import { IconButton } from './icon-button';

describe('IconButton', () => {
  let fixture: ComponentFixture<IconButton>;
  let component: IconButton;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [IconButton],
    }).compileComponents();

    fixture = TestBed.createComponent(IconButton);

    fixture.componentRef.setInput('ariaLabel', 'Edit patient');

    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('renders as a native button', () => {
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button).not.toBeNull();
  });

  it('applies the accessible label', () => {
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.getAttribute('aria-label')).toBe('Edit patient');
  });

  it('uses button type by default', () => {
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.type).toBe('button');
  });

  it('disables the native button', () => {
    fixture.componentRef.setInput('disabled', true);

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.disabled).toBe(true);
  });

  it('applies the requested variant', () => {
    fixture.componentRef.setInput('variant', 'danger');

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.classList.contains('ct-icon-button--danger')).toBe(true);
  });

  it('applies the requested size', () => {
    fixture.componentRef.setInput('size', 'sm');

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.classList.contains('ct-icon-button--sm')).toBe(true);
  });
});
