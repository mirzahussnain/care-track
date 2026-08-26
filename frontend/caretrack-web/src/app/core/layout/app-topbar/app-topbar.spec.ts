import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AppTopbar } from './app-topbar';

describe('AppTopbar', () => {
  let component: AppTopbar;
  let fixture: ComponentFixture<AppTopbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppTopbar],
    }).compileComponents();

    fixture = TestBed.createComponent(AppTopbar);
    fixture.componentRef.setInput('areaLabel', 'Dashboard');
    fixture.componentRef.setInput('accountName', 'Amina Khan');
    fixture.componentRef.setInput('accountRoleLabel', 'Clinician');
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('renders the current area label', () => {
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('Dashboard');
  });

  it('emits a mobile menu request', () => {
    const emitSpy = vi.spyOn(component.mobileMenuOpen, 'emit');
    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    button.click();
    expect(emitSpy).toHaveBeenCalledOnce();
  });

  it('keeps route context compact without a repeated workspace subtitle', () => {
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Dashboard');
    expect(element.textContent).not.toContain('CareTrack Workspace');
    expect(element.querySelector('.app-topbar__context')?.textContent).toContain('Dashboard');
  });

  it('renders account context and emits sign out', () => {
    const emitSpy = vi.spyOn(component.signOut, 'emit');
    const element = fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Amina Khan');
    expect(element.textContent).toContain('Clinician');

    const signOutButton = element.querySelector<HTMLButtonElement>(
      '[data-testid="topbar-sign-out"]',
    )!;
    expect(signOutButton.type).toBe('button');
    expect(signOutButton.textContent).toContain('Sign out');
    signOutButton.focus();
    expect(document.activeElement).toBe(signOutButton);
    signOutButton.click();

    expect(emitSpy).toHaveBeenCalledOnce();
  });
});
