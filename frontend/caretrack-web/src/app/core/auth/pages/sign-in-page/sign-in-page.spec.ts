import { ComponentFixture, TestBed } from '@angular/core/testing';

import { InteractionStatus } from '@azure/msal-browser';

import { MsalBroadcastService, MsalService } from '@azure/msal-angular';

import { BehaviorSubject } from 'rxjs';

import { provideRouter, Router } from '@angular/router';

import { SignInPage } from './sign-in-page';

describe('SignInPage', () => {
  let fixture: ComponentFixture<SignInPage>;
  let component: SignInPage;

  let interactionStatus$: BehaviorSubject<InteractionStatus>;

  const loginRedirect = vi.fn();

  const activeAccount = vi.fn();

  const msalServiceMock = {
    loginRedirect,

    instance: {
      getActiveAccount: activeAccount,
    },
  };

  beforeEach(async () => {
    interactionStatus$ = new BehaviorSubject<InteractionStatus>(InteractionStatus.Startup);

    loginRedirect.mockReset();
    activeAccount.mockReset();
    activeAccount.mockReturnValue(null);

    await TestBed.configureTestingModule({
      imports: [SignInPage],
      providers: [
        provideRouter([
          {
            path: 'dashboard',
            component: SignInPage,
          },
        ]),
        {
          provide: MsalService,
          useValue: msalServiceMock,
        },
        {
          provide: MsalBroadcastService,
          useValue: {
            inProgress$: interactionStatus$,
          },
        },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(SignInPage);

    component = fixture.componentInstance;

    fixture.detectChanges();
  });

  it('creates', () => {
    expect(component).toBeTruthy();
  });

  it('allows sign-in when no interaction is in progress', () => {
    interactionStatus$.next(InteractionStatus.None);
    fixture.detectChanges();
    expect(component.canSignIn()).toBe(true);
  });

  it('prevents sign-in while MSAL interaction is active', () => {
    interactionStatus$.next(InteractionStatus.AcquireToken);

    fixture.detectChanges();

    component.signIn();

    expect(loginRedirect).not.toHaveBeenCalled();
  });

  it('calls loginRedirect when sign-in is allowed', () => {
    interactionStatus$.next(InteractionStatus.None);
    fixture.detectChanges();
    component.signIn();

    expect(loginRedirect).toHaveBeenCalledOnce();
  });

  it('disables the sign-in button during an active interaction', () => {
    interactionStatus$.next(InteractionStatus.AcquireToken);

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.disabled).toBe(true);
  });

  it('enables sign-in when MSAL interaction is complete', () => {
    interactionStatus$.next(InteractionStatus.None);

    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;

    expect(button.disabled).toBe(false);
  });
  it('redirects authenticated users to dashboard', async () => {
    const router = TestBed.inject(Router);

    const navigateSpy = vi.spyOn(router, 'navigate');

    activeAccount.mockReturnValue({
      homeAccountId: 'test',
      environment: 'test',
      tenantId: 'test',
      username: 'user@example.com',
      localAccountId: 'test',
      name: 'Test User',
    });

    interactionStatus$.next(InteractionStatus.None);

    fixture.detectChanges();

    await fixture.whenStable();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  });

  it('does not start sign-in while another MSAL interaction is active', () => {
    interactionStatus$.next(InteractionStatus.HandleRedirect);

    fixture.detectChanges();

    component.signIn();

    expect(loginRedirect).not.toHaveBeenCalled();
  });

  it('renders the supplied CareTrack SVG and only the Microsoft sign-in action', () => {
    const element = fixture.nativeElement as HTMLElement;
    const logo = element.querySelector<HTMLImageElement>('img[src="/brand/caretrack-symbol.svg"]');
    const buttons = element.querySelectorAll('button');

    expect(logo).toBeTruthy();
    expect(logo?.hasAttribute('width')).toBe(false);
    expect(logo?.hasAttribute('height')).toBe(false);
    expect(element.textContent).toContain('Clinical Referral & Workflow Management');
    expect(buttons).toHaveLength(1);
    expect(buttons[0].textContent).toContain('Sign in with Microsoft');
  });

  it('exposes a visible busy state while MSAL interaction is active', () => {
    interactionStatus$.next(InteractionStatus.AcquireToken);
    fixture.detectChanges();

    const button = fixture.nativeElement.querySelector('button') as HTMLButtonElement;
    expect(button.getAttribute('aria-busy')).toBe('true');
    expect(button.textContent).toContain('Loading');
  });
});
