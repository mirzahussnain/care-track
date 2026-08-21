import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppSidebar } from './app-sidebar';

@Component({
  template: '',
})
class TestDashboardPage {}
describe('AppSidebar', () => {
  let component: AppSidebar;
  let fixture: ComponentFixture<AppSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppSidebar],
      providers: [provideRouter([
           {
      path: 'dashboard',
      component: TestDashboardPage,
    },
      ])],
    }).compileComponents();

    fixture = TestBed.createComponent(AppSidebar);

    fixture.componentRef.setInput('navigation', [
      {
        label: 'Dashboard',
        route: '/dashboard',
        icon: 'ph-squares-four',
        exact: true,
      },
    ]);

    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('renders the provided navigation item', () => {
    const element =
      fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain('Dashboard');
  });

  it('emits a collapse request when the toggle is clicked', () => {
  const emitSpy = vi.spyOn(
    component.collapseToggle,
    'emit'
  );

  const button =
    fixture.nativeElement.querySelector(
      'button'
    ) as HTMLButtonElement;

  button.click();

  expect(emitSpy).toHaveBeenCalledOnce();
});

it('emits navigationSelected when a nav item is clicked', async () => {
  const emitSpy = vi.spyOn(
    component.navigationSelected,
    'emit'
  );

  const link =
    fixture.nativeElement.querySelector(
      'a'
    ) as HTMLAnchorElement;

  link.click();

  await fixture.whenStable();

  expect(emitSpy).toHaveBeenCalledOnce();
});

it('shows branding by default', () => {
  const element =
    fixture.nativeElement as HTMLElement;

  expect(element.textContent).toContain('CareTrack');
  expect(element.textContent).toContain(
    'Clinical operations'
  );
});

it('hides branding when showBrand is false', () => {
  fixture.componentRef.setInput(
    'showBrand',
    false
  );

  fixture.detectChanges();

  const element =
    fixture.nativeElement as HTMLElement;

  expect(element.textContent).not.toContain(
    'Clinical operations'
  );
});

it('hides the collapse control when requested', () => {
  fixture.componentRef.setInput(
    'showCollapseControl',
    false
  );

  fixture.detectChanges();

const button =
  fixture.nativeElement.querySelector(
    'button[aria-controls="primary-navigation"]'
  );
  expect(button).toBeNull();
});

});