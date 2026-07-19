import { Component, OnDestroy, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SignalRService } from '../../../../core/services/signalr.service';
import { BidPlacedEvent } from '../../../../core/models/notification.model';

@Component({
  selector: 'app-bid-feed',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bid-feed">
      <h3>Live Bids</h3>
      <ul class="bid-list">
        <li *ngFor="let bid of bids; trackBy: trackById" class="bid-item">
          <span class="bidder">{{ bid.bidderUsername }}</span>
          <span class="amount">\${{ bid.amount | number:'1.2-2' }}</span>
          <span class="time">{{ bid.placedAt | date:'shortTime' }}</span>
        </li>
        <li *ngIf="bids.length === 0" class="empty">No bids yet</li>
      </ul>
    </div>
  `,
  styles: [`
    .bid-feed { display: flex; flex-direction: column; gap: 0.5rem; }
    .bid-list { list-style: none; margin: 0; padding: 0; max-height: 300px; overflow-y: auto; }
    .bid-item { display: flex; justify-content: space-between; padding: 0.5rem; border-bottom: 1px solid #e0e0e0; }
    .bidder { font-weight: 500; }
    .amount { color: #2e7d32; font-weight: 700; }
    .time { color: #757575; font-size: 0.875rem; }
    .empty { color: #9e9e9e; padding: 0.5rem; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BidFeedComponent implements OnInit, OnDestroy {
  bids: BidPlacedEvent[] = [];
  private destroy$ = new Subject<void>();

  constructor(private readonly signalR: SignalRService) {}

  ngOnInit(): void {
    this.signalR.bidPlaced$
      .pipe(takeUntil(this.destroy$))
      .subscribe((bids) => {
        this.bids = bids;
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackById(index: number, bid: BidPlacedEvent): string {
    return bid.bidId;
  }
}
