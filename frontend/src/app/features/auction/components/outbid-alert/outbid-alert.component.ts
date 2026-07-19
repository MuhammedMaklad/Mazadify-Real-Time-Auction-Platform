import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { SignalRService } from '../../../../core/services/signalr.service';

@Component({
  selector: 'app-outbid-alert',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './outbid-alert.component.html',
  styleUrl: './outbid-alert.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class OutbidAlertComponent implements OnInit, OnDestroy {
  /** Only react to outbid events for this specific auction */
  @Input({ required: true }) auctionId!: string;

  visible = false;
  currentHighestBid = 0;

  private dismissTimer: ReturnType<typeof setTimeout> | null = null;
  private destroy$ = new Subject<void>();

  constructor(
    private readonly signalR: SignalRService,
    private readonly cdr: ChangeDetectorRef
  ) { }

  ngOnInit(): void {
    this.signalR.outbid$
      .pipe(
        takeUntil(this.destroy$),
        filter((event) => event?.auctionId === this.auctionId)
      )
      .subscribe((event) => {
        if (!event) return;
        this.currentHighestBid = event.newAmount;
        this.showBanner();
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.clearTimer();
  }

  dismiss(): void {
    this.visible = false;
    this.clearTimer();
    this.cdr.markForCheck();
  }

  private showBanner(): void {
    this.visible = true;
    this.clearTimer();
    // Auto-dismiss after 8 seconds
    this.dismissTimer = setTimeout(() => {
      this.visible = false;
      this.dismissTimer = null;
      this.cdr.markForCheck();
    }, 8000);
    this.cdr.markForCheck();
  }

  private clearTimer(): void {
    if (this.dismissTimer) {
      clearTimeout(this.dismissTimer);
      this.dismissTimer = null;
    }
  }
}
