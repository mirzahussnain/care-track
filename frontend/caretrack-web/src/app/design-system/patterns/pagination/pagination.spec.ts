import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Pagination } from './pagination';

describe('Pagination', () => {
  let fixture: ComponentFixture<Pagination>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({ imports: [Pagination] }).compileComponents();
    fixture = TestBed.createComponent(Pagination);
    fixture.componentRef.setInput('page', 1);
    fixture.componentRef.setInput('pageSize', 20);
    fixture.componentRef.setInput('totalCount', 95);
    fixture.componentRef.setInput('totalPages', 5);
  });

  it('disables previous on the first page and reports the result range', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-label="Go to previous page"]').disabled).toBe(
      true,
    );
    expect(fixture.nativeElement.textContent).toContain('Showing 1–20 of 95');
  });

  it('marks the current page accessibly', () => {
    fixture.componentRef.setInput('page', 3);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-current="page"]')?.textContent.trim()).toBe(
      '3',
    );
  });

  it('uses a compact page window for large result sets', () => {
    fixture.componentRef.setInput('page', 50);
    fixture.componentRef.setInput('totalPages', 100);
    fixture.componentRef.setInput('totalCount', 2000);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('.ct-pagination__button--page').length).toBe(5);
    expect(fixture.nativeElement.querySelectorAll('.ct-pagination__ellipsis').length).toBe(2);
  });

  it('emits previous, next, and numbered page changes', () => {
    fixture.componentRef.setInput('page', 3);
    fixture.detectChanges();
    const emitted: number[] = [];
    fixture.componentInstance.pageChange.subscribe((page) => emitted.push(page));
    fixture.nativeElement.querySelector('[aria-label="Go to previous page"]').click();
    fixture.nativeElement.querySelector('[aria-label="Go to next page"]').click();
    fixture.nativeElement.querySelector('[aria-label="Go to page 1"]').click();
    expect(emitted).toEqual([2, 4, 1]);
  });

  it('disables next on the final page', () => {
    fixture.componentRef.setInput('page', 5);
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelector('[aria-label="Go to next page"]').disabled).toBe(
      true,
    );
  });

  it('renders a zero result state without page buttons', () => {
    fixture.componentRef.setInput('page', 1);
    fixture.componentRef.setInput('totalCount', 0);
    fixture.componentRef.setInput('totalPages', 0);
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Showing 0–0 of 0');
    expect(fixture.nativeElement.querySelectorAll('.ct-pagination__button--page').length).toBe(0);
  });
});
