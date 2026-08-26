import { Component } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PageHeader } from './page-header';

describe('PageHeader', () => {
  let fixture: ComponentFixture<PageHeader>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [PageHeader] }).compileComponents();
    fixture = TestBed.createComponent(PageHeader);
    fixture.componentRef.setInput('title', 'Referrals');
  });

  it('renders its title as an h1', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('h1')?.textContent).toContain('Referrals');
  });

  it('renders the description only when supplied', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('.ct-page-header__description')).toBeNull();
    fixture.componentRef.setInput('description', 'Review and coordinate current work.');
    fixture.detectChanges();
    expect(
      fixture.nativeElement.querySelector('.ct-page-header__description')?.textContent,
    ).toContain('Review and coordinate current work.');
  });
});

@Component({
  imports: [PageHeader],
  template: '<ct-page-header title="Referrals"><button>New referral</button></ct-page-header>',
})
class PageHeaderHost {}

describe('PageHeader projection', () => {
  it('renders projected actions', async () => {
    await TestBed.configureTestingModule({ imports: [PageHeaderHost] }).compileComponents();
    const fixture = TestBed.createComponent(PageHeaderHost);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('button')?.textContent).toContain('New referral');
  });
});
