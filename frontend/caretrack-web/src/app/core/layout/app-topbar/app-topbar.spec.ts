import {
  ComponentFixture,
  TestBed,
} from '@angular/core/testing';

import { AppTopbar } from './app-topbar';

describe('AppTopbar', () => {
  let component: AppTopbar;
  let fixture: ComponentFixture<AppTopbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AppTopbar],
    }).compileComponents();

    fixture = TestBed.createComponent(AppTopbar);

    fixture.componentRef.setInput(
      'areaLabel',
      'Dashboard'
    );

    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('renders the current area label', () => {
    const element =
      fixture.nativeElement as HTMLElement;

    expect(element.textContent).toContain(
      'Dashboard'
    );
  });

  it('emits a mobile menu request', () => {
  const emitSpy = vi.spyOn(
    component.mobileMenuOpen,
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