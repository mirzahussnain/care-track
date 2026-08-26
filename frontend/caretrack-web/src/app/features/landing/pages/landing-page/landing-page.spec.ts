import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { MsalService } from '@azure/msal-angular';
import { AccountInfo } from '@azure/msal-browser';
import { vi } from 'vitest';

import { LandingPage } from './landing-page';

describe('LandingPage', () => {
  let activeAccount: AccountInfo | null;
  let cachedAccounts: AccountInfo[];

  const account = {
    homeAccountId: 'home-account-id',
    environment: 'login.microsoftonline.com',
    tenantId: 'tenant-id',
    username: 'clinician@example.test',
    localAccountId: 'local-account-id',
    name: 'CareTrack Clinician',
  } as AccountInfo;

  beforeEach(async () => {
    activeAccount = null;
    cachedAccounts = [];

    await TestBed.configureTestingModule({
      imports: [LandingPage],
      providers: [
        provideRouter([]),
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: MsalService,
          useValue: {
            instance: {
              getActiveAccount: vi.fn(() => activeAccount),
              getAllAccounts: vi.fn(() => cachedAccounts),
            },
          },
        },
      ],
    }).compileComponents();
  });

  function createPage(): ComponentFixture<LandingPage> {
    const fixture = TestBed.createComponent(LandingPage);
    fixture.detectChanges();
    return fixture;
  }

  it('renders as a data-free public page while signed out', () => {
    const fixture = createPage();
    const http = TestBed.inject(HttpTestingController);

    expect(fixture.componentInstance).toBeTruthy();
    http.verify();
  });

  it('does not request a user or clinical API when a cached account is present', () => {
    cachedAccounts = [account];
    createPage();

    TestBed.inject(HttpTestingController).verify();
  });

  it('shows only signed-out CTAs when no MSAL account is cached', () => {
    const element = createPage().nativeElement as HTMLElement;
    const authLinks = Array.from(element.querySelectorAll<HTMLAnchorElement>('[data-auth-cta]'));

    expect(element.textContent).toContain('Sign in');
    expect(element.textContent).not.toContain('Open Dashboard');
    expect(authLinks.length).toBeGreaterThanOrEqual(4);
    expect(authLinks.every((link) => link.getAttribute('href') === '/auth/sign-in')).toBe(true);
    expect(element.querySelector('[data-auth-cta="hero"]')?.textContent).toContain(
      'Sign in to CareTrack',
    );
  });

  it('shows only dashboard CTAs when an MSAL account is cached', () => {
    activeAccount = account;
    const element = createPage().nativeElement as HTMLElement;
    const authLinks = Array.from(element.querySelectorAll<HTMLAnchorElement>('[data-auth-cta]'));

    expect(element.textContent).toContain('Open Dashboard');
    expect(element.textContent).not.toContain('Sign in');
    expect(authLinks.every((link) => link.getAttribute('href') === '/dashboard')).toBe(true);
  });

  it('uses one semantic page heading and the real CareTrack brand and screenshots', () => {
    const element = createPage().nativeElement as HTMLElement;
    const imageSources = Array.from(element.querySelectorAll<HTMLImageElement>('img')).map(
      (image) => image.getAttribute('src'),
    );

    expect(element.querySelector('nav[aria-label="Public navigation"]')).toBeTruthy();
    expect(element.querySelector('main')).toBeTruthy();
    expect(element.querySelector('footer')).toBeTruthy();
    expect(element.querySelectorAll('h1')).toHaveLength(1);
    expect(
      element.querySelector<HTMLImageElement>('img[src="/brand/caretrack-symbol.svg"]')?.alt,
    ).toBe('CareTrack');
    expect(imageSources).toContain('/product-demos/dashboard-overview.png');
    expect(imageSources).toContain('/product-demos/Appointments-Page.png');
    expect(imageSources).toContain('/product-demos/Referrals-Details.png');
  });

  it('keeps the hero screenshot eager and all product evidence meaningfully labelled', () => {
    const element = createPage().nativeElement as HTMLElement;
    const heroImage = element.querySelector<HTMLImageElement>(
      'img[src="/product-demos/dashboard-overview.png"]',
    );
    const productImages = Array.from(
      element.querySelectorAll<HTMLImageElement>('img[src^="/product-demos/"]'),
    );

    expect(heroImage?.getAttribute('loading')).toBeNull();
    expect(heroImage?.getAttribute('fetchpriority')).toBe('high');
    expect(heroImage?.getAttribute('width')).toBe('1917');
    expect(productImages.every((image) => Boolean(image.alt))).toBe(true);
    expect(
      productImages.filter((image) => image.getAttribute('loading') === 'lazy').length,
    ).toBeGreaterThanOrEqual(3);
  });

  it('offers an explicit workflow CTA without commercial trial or pricing language', () => {
    const element = createPage().nativeElement as HTMLElement;
    const text = element.textContent?.toLowerCase() ?? '';

    expect(element.querySelector<HTMLAnchorElement>('a[href="#workflow"]')?.textContent).toContain(
      'Workflow',
    );
    expect(text).not.toContain('start free trial');
    expect(text).not.toContain('pricing');
    expect(text).not.toContain('subscribe');
    expect(text).not.toContain('book demo');
  });

  it('supports click and arrow-key navigation across the manual product gallery', () => {
    const fixture = createPage();
    const element = fixture.nativeElement as HTMLElement;
    const tabs = Array.from(element.querySelectorAll<HTMLButtonElement>('[role="tab"]'));

    expect(tabs).toHaveLength(4);
    expect(tabs[0].getAttribute('aria-selected')).toBe('true');

    tabs[2].click();
    fixture.detectChanges();
    expect(tabs[2].getAttribute('aria-selected')).toBe('true');
    expect(element.querySelector('[role="tabpanel"] img')?.getAttribute('alt')).toContain(
      'referral detail',
    );

    tabs[2].dispatchEvent(new KeyboardEvent('keydown', { key: 'ArrowRight', bubbles: true }));
    fixture.detectChanges();
    expect(tabs[3].getAttribute('aria-selected')).toBe('true');
  });

  it('exposes one accessible mobile menu control and closes the menu with Escape', () => {
    const fixture = createPage();
    const element = fixture.nativeElement as HTMLElement;
    const menuButton = element.querySelector<HTMLButtonElement>(
      '[aria-controls="public-mobile-menu"]',
    );

    expect(element.querySelectorAll('[aria-controls="public-mobile-menu"]')).toHaveLength(1);
    expect(menuButton?.parentElement?.lastElementChild).toBe(menuButton);
    expect(element.querySelector('[data-auth-cta="mobile"]')).toBeNull();
    expect(menuButton?.getAttribute('aria-expanded')).toBe('false');
    menuButton?.click();
    fixture.detectChanges();
    expect(menuButton?.getAttribute('aria-expanded')).toBe('true');
    expect(element.querySelector('#public-mobile-menu')).toBeTruthy();
    expect(
      element.querySelector('#public-mobile-menu [data-auth-cta="mobile"]')?.textContent,
    ).toContain('Sign in');

    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' }));
    fixture.detectChanges();
    expect(menuButton?.getAttribute('aria-expanded')).toBe('false');
  });

  it('provides useful footer navigation and an accurate portfolio disclaimer', () => {
    const footer = createPage().nativeElement.querySelector('footer') as HTMLElement;

    expect(footer.textContent).toContain('Product');
    expect(footer.textContent).toContain('Application');
    expect(footer.textContent).toContain('Project');
    expect(footer.textContent).toContain('Uses synthetic demonstration data.');
    expect(footer.textContent).not.toContain('Privacy Policy');
    expect(footer.textContent).not.toContain('Terms');
    expect(footer.textContent).not.toContain('Contact Sales');
  });
});
