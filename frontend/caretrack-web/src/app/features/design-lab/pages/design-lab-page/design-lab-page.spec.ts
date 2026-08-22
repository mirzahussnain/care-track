import { TestBed } from '@angular/core/testing';
import { DesignLabPage } from './design-lab-page';

describe('DesignLabPage', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DesignLabPage],
    }).compileComponents();
  });

  it('renders only the initial quiet clinical direction', () => {
    const fixture = TestBed.createComponent(DesignLabPage);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;

    expect(fixture.componentInstance.activeDirection()).toBe('quiet');
    expect(element.querySelector('app-quiet-clinical')).toBeTruthy();
    expect(element.querySelector('app-editorial-operations')).toBeNull();
    expect(element.querySelector('app-structured-modern')).toBeNull();
  });

  it('renders the selected direction and removes the previous direction', () => {
    const fixture = TestBed.createComponent(DesignLabPage);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    const editorialButton = element.querySelector(
      '[data-direction="editorial"]',
    ) as HTMLButtonElement;

    editorialButton.click();
    fixture.detectChanges();

    expect(fixture.componentInstance.activeDirection()).toBe('editorial');
    expect(element.querySelector('app-editorial-operations')).toBeTruthy();
    expect(element.querySelector('app-quiet-clinical')).toBeNull();
    expect(element.querySelector('app-structured-modern')).toBeNull();
  });

  it('renders the reusable component verification gallery', () => {
    const fixture = TestBed.createComponent(DesignLabPage);
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;

    expect(element.querySelector('ct-page-header')).toBeTruthy();
    expect(element.querySelector('ct-status-chip')).toBeTruthy();
    expect(element.querySelector('ct-surface')).toBeTruthy();
    expect(element.querySelector('ct-empty-state')).toBeTruthy();
    expect(element.querySelector('ct-skeleton')).toBeTruthy();
    expect(element.querySelector('ct-form-field')).toBeTruthy();
    expect(element.querySelector('ct-data-toolbar')).toBeTruthy();
  });
});
