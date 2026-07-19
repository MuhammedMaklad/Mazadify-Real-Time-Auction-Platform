import {
  Component,
  OnDestroy,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { SignalRService } from '../../../../core/services/signalr.service';
import { BidPlacedEvent } from '../../../../core/models/notification.model';

@Component({
  selector: 'app-bid-feed',
  standalone: true,
  imports: [CommonModule, CurrencyPipe, DatePipe],
  templateUrl: './bid-feed.component.html',
  styleUrl: './bid-feed.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class BidFeedComponent implements OnInit, OnDestroy {
  bids: BidPlacedEvent[] = [];
  private destroy$ = new Subject<void>();

  constructor(
    private readonly signalR: SignalRService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.signalR.bidPlaced$
      .pipe(takeUntil(this.destroy$))
      .subscribe((bids) => {
        this.bids = bids;
        this.cdr.markForCheck();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  trackById(_index: number, bid: BidPlacedEvent): string {
    return bid.bidId;
  }
}
