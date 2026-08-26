import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Button } from './button';

describe('Button', () => {
  let fixture: ComponentFixture<Button>;
  let component: Button;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Button],
    }).compileComponents();

    fixture = TestBed.createComponent(Button);
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

  it('uses button type by default', () => {
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.type).toBe('button');
  });

  it('applies the requested button type', () => {
    fixture.componentRef.setInput('type', 'submit');

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.type).toBe('submit');
  });

  it('disables the native button when disabled', () => {
    fixture.componentRef.setInput('disabled', true);

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.disabled).toBe(true);
  });

  it('disables the native button while loading', () => {
    fixture.componentRef.setInput('loading', true);

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.disabled).toBe(true);
    expect(button.getAttribute('aria-busy')).toBe('true');
  });

  it('applies the selected variant', () => {
    fixture.componentRef.setInput('variant', 'danger');

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.classList.contains('ct-button--danger')).toBe(true);
  });

  it.each(['primary', 'secondary', 'ghost', 'success', 'warning', 'danger'] as const)(
    'supports the %s semantic variant',
    (variant) => {
      fixture.componentRef.setInput('variant', variant);
      fixture.detectChanges();

      const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
      expect(button.classList.contains('ct-button--' + variant)).toBe(true);
    },
  );
});
