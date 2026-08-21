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
    const buttons = element.querySelectorAll('button');

    buttons[1].click();
    fixture.detectChanges();

    expect(fixture.componentInstance.activeDirection()).toBe('editorial');
    expect(element.querySelector('app-editorial-operations')).toBeTruthy();
    expect(element.querySelector('app-quiet-clinical')).toBeNull();
    expect(element.querySelector('app-structured-modern')).toBeNull();
  });
});
