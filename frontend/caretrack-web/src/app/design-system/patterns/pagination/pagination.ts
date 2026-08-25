import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

interface PageNumberItem {
  readonly kind: 'page';
  readonly key: string;
  readonly value: number;
}

interface EllipsisItem {
  readonly kind: 'ellipsis';
  readonly key: string;
}

type PaginationItem = PageNumberItem | EllipsisItem;

@Component({
  selector: 'ct-pagination',
  standalone: true,
  templateUrl: './pagination.html',
  styleUrl: './pagination.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class Pagination {
  readonly page = input.required<number>();
  readonly pageSize = input.required<number>();
  readonly totalCount = input.required<number>();
  readonly totalPages = input.required<number>();
  readonly ariaLabel = input('Pagination');
  readonly pageChange = output<number>();

  readonly firstResult = computed(() =>
    this.totalCount() === 0 ? 0 : (this.page() - 1) * this.pageSize() + 1,
  );

  readonly lastResult = computed(() => Math.min(this.page() * this.pageSize(), this.totalCount()));

  readonly items = computed<readonly PaginationItem[]>(() => {
    const totalPages = this.totalPages();
    const current = this.page();

    if (totalPages <= 0) {
      return [];
    }

    const pages = new Set<number>([1, totalPages]);
    for (let value = current - 1; value <= current + 1; value += 1) {
      if (value > 1 && value < totalPages) {
        pages.add(value);
      }
    }

    const ordered = [...pages].sort((left, right) => left - right);
    const items: PaginationItem[] = [];

    ordered.forEach((value, index) => {
      const previous = ordered[index - 1];
      if (previous !== undefined && value - previous > 1) {
        items.push({ kind: 'ellipsis', key: `ellipsis-${previous}-${value}` });
      }
      items.push({ kind: 'page', key: `page-${value}`, value });
    });

    return items;
  });

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }
    this.pageChange.emit(page);
  }
}
