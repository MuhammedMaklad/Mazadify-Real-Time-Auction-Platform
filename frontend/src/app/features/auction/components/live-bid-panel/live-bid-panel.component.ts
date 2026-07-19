import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { AsyncPipe, CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';

import { SignalRService, ConnectionState } from '../../../../core/services/signalr.service';
import { BidDto, PendingBid } from '../../../../core/models/bid.model';

import { HighestBidDisplayComponent } from '../highest-bid-display/highest-bid-display.component';
import { PlaceBidFormComponent } from '../place-bid-form/place-bid-form.component';
import { BidHistoryTableComponent } from '../bid-history-table/bid-history-table.component';
import { OutbidAlertComponent } from '../outbid-alert/outbid-alert.component';
import { CountdownComponent } from '../countdown/countdown.component';

/** Minimal auction shape this panel needs — matches AuctionDto from the backend */
export interface AuctionBrief {
  id: string;
  startingPrice: number;
  currentHighestBid: number;
  bidIncrement: number;
  endTime: string;
  status: string;
}

@Component({
  selector: 'app-live-bid-panel',
  standalone: true,
  imports: [
    CommonModule,
    AsyncPipe,
    HighestBidDisplayComponent,
    PlaceBidFormComponent,
    BidHistoryTableComponent,
    OutbidAlertComponent,
    CountdownComponent
  ],
  templateUrl: './live-bid-panel.component.html',
  styleUrl: './live-bid-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LiveBidPanelComponent implements OnInit, OnDestroy {
  /** Pass the auction object from the detail page */
  @Input({ required: true }) auction!: AuctionBrief;

  currentHighestBid = 0;
  pendingBid: PendingBid | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly signalR: SignalRService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  get connectionState$() {
    return this.signalR.connectionState$;
  }

  get auctionEnded(): boolean {
    return this.auction.status === 'Ended' || this.auction.status === 'Cancelled';
  }

  ngOnInit(): void {
    // Seed the current highest bid from the auction snapshot
    this.currentHighestBid = this.auction.currentHighestBid ?? 0;

    // Join SignalR group for this auction
    this.signalR.start().then(() => {
      this.signalR.joinAuctionGroup(this.auction.id);
    });

    // Listen for live bid updates and update local highest bid
    this.signalR.bidPlaced$
      .pipe(
        takeUntil(this.destroy$),
        filter((events) => events.length > 0 && events[0].auctionId === this.auction.id)
      )
      .subscribe((events) => {
        const latest = events[0];
        if (latest.amount > this.currentHighestBid) {
          this.currentHighestBid = latest.amount;

          // If a confirmed bid came in that matches the pending optimistic one, clear it
          if (this.pendingBid && latest.amount === this.pendingBid.amount) {
            this.pendingBid = null;
          }

          this.cdr.markForCheck();
        }
      });
  }

  ngOnDestroy(): void {
    this.signalR.leaveAuctionGroup(this.auction.id);
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Form event handlers ───────────────────────────────────────────────────

  onOptimisticBid(amount: number): void {
    this.pendingBid = {
      tempId: crypto.randomUUID(),
      amount,
      placedAt: new Date().toISOString(),
      pending: true
    };
    this.cdr.markForCheck();
  }

  onOptimisticRollback(): void {
    this.pendingBid = null;
    this.cdr.markForCheck();
  }

  onBidPlaced(bid: BidDto): void {
    // Update highest bid immediately from server-confirmed data
    if (bid.amount > this.currentHighestBid) {
      this.currentHighestBid = bid.amount;
    }
    // Clear pending — SignalR will deliver the real row
    this.pendingBid = null;
    this.cdr.markForCheck();
  }

  connectionLabel(status: ConnectionState['status']): string {
    const labels: Record<ConnectionState['status'], string> = {
      connected: 'Live',
      connecting: 'Connecting…',
      reconnecting: 'Reconnecting…',
      disconnected: 'Offline'
    };
    return labels[status];
  }
}
