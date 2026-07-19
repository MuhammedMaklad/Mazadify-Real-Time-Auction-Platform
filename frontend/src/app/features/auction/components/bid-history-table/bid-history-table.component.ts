import {
  Component,
  Input,
  OnInit,
  OnChanges,
  OnDestroy,
  SimpleChanges,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { BidApiService } from '../../../../core/services/bid-api.service';
import { SignalRService } from '../../../../core/services/signalr.service';
import { BidDto, PendingBid, BidRow } from '../../../../core/models/bid.model';

@Component({
  selector: 'app-bid-history-table',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './bid-history-table.component.html',
  styleUrl: './bid-history-table.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BidHistoryTableComponent implements OnInit, OnChanges, OnDestroy {
  @Input({ required: true }) auctionId!: string;
  /** Pending optimistic bid injected by parent */
  @Input() pendingBid: PendingBid | null = null;

  rows: BidRow[] = [];
  loading = false;
  page = 1;
  readonly pageSize = 10;
  totalCount = 0;
  totalPages = 1;

  private confirmedBids: BidDto[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private readonly bidApi: BidApiService,
    private readonly signalR: SignalRService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.loadPage(1);
    this.listenToSignalR();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['pendingBid']) {
      this.buildRows();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages || this.loading) return;
    this.loadPage(p);
  }

  asBid(row: BidRow): BidDto {
    return row as BidDto;
  }

  isTopBid(row: BidRow): boolean {
    if (row.pending) return false;
    return this.rows.findIndex((r) => !r.pending) === this.rows.indexOf(row);
  }

  trackRow(_: number, row: BidRow): string {
    return row.pending ? row.tempId : (row as BidDto).id;
  }

  private loadPage(p: number): void {
    this.loading = true;
    this.cdr.markForCheck();

    this.bidApi
      .getBidHistory(this.auctionId, p, this.pageSize)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (result) => {
          this.page = result.page;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.confirmedBids = result.items;
          this.loading = false;
          this.buildRows();
        },
        error: () => {
          this.loading = false;
          this.cdr.markForCheck();
        }
      });
  }

  private listenToSignalR(): void {
    this.signalR.bidPlaced$
      .pipe(takeUntil(this.destroy$))
      .subscribe((events) => {
        if (!events.length || this.page !== 1) return;

        const latest = events[0];
        const alreadyExists = this.confirmedBids.some(
          (b) => b.id === latest.bidId
        );
        if (alreadyExists) return;

        const incoming: BidDto = {
          id: latest.bidId,
          auctionId: latest.auctionId,
          bidderId: latest.bidderId,
          amount: latest.amount,
          status: 'Active',
          placedAt: latest.placedAt,
          isAutoBid: false,
          bidder: { id: latest.bidderId, username: latest.bidderUsername }
        };

        this.confirmedBids = [incoming, ...this.confirmedBids].slice(
          0,
          this.pageSize
        );
        this.totalCount += 1;
        this.buildRows();
      });
  }

  private buildRows(): void {
    const confirmed: BidRow[] = this.confirmedBids.map((b) => ({
      ...b,
      pending: false as const
    }));

    this.rows = this.pendingBid ? [this.pendingBid, ...confirmed] : confirmed;
    this.cdr.markForCheck();
  }
}
