import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';

import { AppSidebar } from './app-sidebar';

describe('AppSidebar', () => {
  let component: AppSidebar;
  let fixture: ComponentFixture<AppSidebar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppSidebar],
      providers: [provideRouter([])],
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


});