import {
  AfterViewInit,
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { MsalService } from '@azure/msal-angular';

import {
  INTERACTIVE_DEMO_ACCOUNTS,
  InteractiveDemoAccount,
} from '../../demo/interactive-demo.config';
import {
  buttonFromEvent,
  restoreFocusIfAvailable,
} from '../../../../shared/utils/focus-management';

interface ProductDemo {
  readonly label: string;
  readonly src: string;
  readonly width: number;
  readonly height: number;
  readonly alt: string;
  readonly description: string;
  readonly highlights: readonly string[];
}

interface LandingAuthCta {
  readonly label: 'Sign in' | 'Open Dashboard';
  readonly prominentLabel: 'Sign in to CareTrack' | 'Open Dashboard';
  readonly route: '/auth/sign-in' | '/dashboard';
}

@Component({
  selector: 'app-landing-page',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './landing-page.html',
  styleUrl: './landing-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LandingPage implements AfterViewInit {
  private readonly title = inject(Title);
  private readonly meta = inject(Meta);
  private readonly msalService = inject(MsalService);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);
  private readonly demoDialog = viewChild<ElementRef<HTMLDialogElement>>('demoDialog');
  private demoDialogTrigger: HTMLButtonElement | null = null;

  readonly mobileMenuOpen = signal(false);
  readonly selectedDemoIndex = signal(0);
  readonly selectedDemoAccount = signal<InteractiveDemoAccount | null>(null);
  readonly passwordVisible = signal(false);
  readonly credentialCopyStatus = signal('');
  readonly headerElevated = signal(false);
  readonly currentYear = new Date().getFullYear();
  readonly authCta: LandingAuthCta = this.resolveAuthCta();
  readonly interactiveDemoAccounts = INTERACTIVE_DEMO_ACCOUNTS;

  readonly demos: readonly ProductDemo[] = [
    {
      label: 'Patients',
      src: '/product-demos/Patients-Page.png',
      width: 1917,
      height: 1000,
      alt: 'CareTrack patient records screen with searchable patient table and a registration action.',
      description:
        'Keep structured patient identity and reference details available to the referral workflow.',
      highlights: ['Searchable records', 'Structured registration', 'Referral-ready context'],
    },
    {
      label: 'Referrals',
      src: '/product-demos/Referrals-Page.png',
      width: 1912,
      height: 997,
      alt: 'CareTrack referral queue showing priority, status, team assignment, and workflow filtering.',
      description:
        'Review operational queues with status, priority, and assignment context kept visible.',
      highlights: ['Priority and status', 'Assignment context', 'Operational filtering'],
    },
    {
      label: 'Referral workflow',
      src: '/product-demos/Referrals-Details.png',
      width: 1911,
      height: 997,
      alt: 'CareTrack referral detail showing patient context, referral status, triage information, and workflow actions.',
      description:
        'Move through permitted referral actions while patient and triage context remain connected.',
      highlights: ['Explicit transitions', 'Triage context', 'Role-aware actions'],
    },
    {
      label: 'Appointments',
      src: '/product-demos/Appointments-Page.png',
      width: 1916,
      height: 987,
      alt: 'CareTrack appointments screen with scheduling filters, patient and referral context, and appointment statuses.',
      description:
        'Coordinate referral-linked appointments through scheduling, check-in, active work, and completion.',
      highlights: ['Referral-linked scheduling', 'Explicit status actions', 'Conflict protection'],
    },
  ];

  get selectedDemo(): ProductDemo {
    return this.demos[this.selectedDemoIndex()];
  }

  constructor() {
    const pageTitle = 'CareTrack — Clinical Referral & Workflow Management';
    const description =
      'A full-stack clinical operations portfolio project for managing patient referrals, appointments, workflow states, and clinical notes with Microsoft Entra ID role-based access.';

    this.title.setTitle(pageTitle);
    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ property: 'og:title', content: pageTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
  }

  ngAfterViewInit(): void {
    const revealTargets = Array.from(
      this.host.nativeElement.querySelectorAll<HTMLElement>('[data-landing-reveal]'),
    );
    const reducedMotion = globalThis.matchMedia?.('(prefers-reduced-motion: reduce)').matches;

    if (reducedMotion || !('IntersectionObserver' in globalThis)) {
      revealTargets.forEach((target) => target.classList.add('is-visible'));
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (!entry.isIntersecting) {
            return;
          }

          entry.target.classList.add('is-visible');
          observer.unobserve(entry.target);
        });
      },
      { rootMargin: '0px 0px -10% 0px', threshold: 0.12 },
    );

    revealTargets.forEach((target) => observer.observe(target));
    this.destroyRef.onDestroy(() => observer.disconnect());
  }

  selectDemo(index: number): void {
    this.selectedDemoIndex.set(index);
  }

  onDemoKeydown(event: KeyboardEvent, index: number): void {
    let nextIndex = index;

    if (event.key === 'ArrowRight' || event.key === 'ArrowDown') {
      nextIndex = (index + 1) % this.demos.length;
    } else if (event.key === 'ArrowLeft' || event.key === 'ArrowUp') {
      nextIndex = (index - 1 + this.demos.length) % this.demos.length;
    } else if (event.key === 'Home') {
      nextIndex = 0;
    } else if (event.key === 'End') {
      nextIndex = this.demos.length - 1;
    } else {
      return;
    }

    event.preventDefault();
    this.selectDemo(nextIndex);
    const tabs = this.host.nativeElement.querySelectorAll<HTMLButtonElement>('[role=tab]');
    tabs[nextIndex]?.focus();
  }

  toggleMobileMenu(): void {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen.set(false);
  }

  openDemoDialog(account: InteractiveDemoAccount, event: MouseEvent): void {
    const dialog = this.demoDialog()?.nativeElement;
    if (!dialog) {
      return;
    }

    this.demoDialogTrigger = buttonFromEvent(event);
    this.selectedDemoAccount.set(account);
    this.passwordVisible.set(false);
    this.credentialCopyStatus.set('');
    dialog.showModal();

    queueMicrotask(() => {
      dialog.querySelector<HTMLButtonElement>('[data-demo-dialog-close]')?.focus();
    });
  }

  closeDemoDialog(): void {
    const dialog = this.demoDialog()?.nativeElement;
    if (dialog?.open) {
      dialog.close();
    }
  }

  onDemoDialogClosed(): void {
    this.selectedDemoAccount.set(null);
    this.passwordVisible.set(false);
    this.credentialCopyStatus.set('');
    restoreFocusIfAvailable(this.demoDialogTrigger);
    this.demoDialogTrigger = null;
  }

  onDemoDialogBackdropClick(event: MouseEvent): void {
    if (event.target === this.demoDialog()?.nativeElement) {
      this.closeDemoDialog();
    }
  }

  togglePasswordVisibility(): void {
    this.passwordVisible.update((visible) => !visible);
  }

  async copyCredential(label: 'Email' | 'Password', value: string): Promise<void> {
    try {
      await globalThis.navigator.clipboard.writeText(value);
      this.credentialCopyStatus.set(`${label} copied.`);
    } catch {
      this.credentialCopyStatus.set(
        `Unable to copy ${label.toLowerCase()}. Select and copy it manually.`,
      );
    }
  }

  continueToSignIn(): void {
    this.closeDemoDialog();
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.demoDialog()?.nativeElement.open) {
      this.closeDemoDialog();
      return;
    }

    this.closeMobileMenu();
  }

  @HostListener('window:scroll')
  onWindowScroll(): void {
    this.headerElevated.set(globalThis.scrollY > 16);
  }

  private resolveAuthCta(): LandingAuthCta {
    const hasCachedAccount =
      this.msalService.instance.getActiveAccount() !== null ||
      this.msalService.instance.getAllAccounts().length > 0;

    return hasCachedAccount
      ? { label: 'Open Dashboard', prominentLabel: 'Open Dashboard', route: '/dashboard' }
      : { label: 'Sign in', prominentLabel: 'Sign in to CareTrack', route: '/auth/sign-in' };
  }
}
