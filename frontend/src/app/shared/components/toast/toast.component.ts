import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { ToastService, ToastMessage } from '../../../core/services/toast.service';
import { SignalRService } from '../../../core/services/signalr.service';
import { OutbidAlertEvent, AuctionStartedEvent, AuctionEndedEvent } from '../../../core/models/notification.model';

@Component({
  selector: 'app-toast',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="toast-container">
      <div
        *ngFor="let toast of messages"
        class="toast"
        [class.info]="toast.type === 'info'"
        [class.success]="toast.type === 'success'"
        [class.warning]="toast.type === 'warning'"
        [class.error]="toast.type === 'error'">
        <div class="toast-title">{{ toast.title }}</div>
        <div class="toast-message">{{ toast.message }}</div>
        <button type="button" class="close" (click)="dismiss(toast.id)">×</button>
      </div>
    </div>
  `,
  styles: [`
    .toast-container { position: fixed; bottom: 1rem; right: 1rem; display: flex; flex-direction: column; gap: 0.5rem; z-index: 1000; }
    .toast { min-width: 280px; padding: 1rem; border-radius: 8px; color: white; box-shadow: 0 4px 12px rgba(0,0,0,0.15); position: relative; }
    .toast.info { background: #1976d2; }
    .toast.success { background: #388e3c; }
    .toast.warning { background: #f57c00; }
    .toast.error { background: #d32f2f; }
    .toast-title { font-weight: 600; margin-bottom: 0.25rem; }
    .toast-message { font-size: 0.875rem; }
    .close { position: absolute; top: 0.25rem; right: 0.5rem; background: transparent; border: none; color: white; font-size: 1.25rem; cursor: pointer; }
  `]
})
export class ToastComponent implements OnInit, OnDestroy {
  messages: ToastMessage[] = [];
  private destroy$ = new Subject<void>();
  private unsubscribe?: (() => void);

  constructor(
    private readonly toastService: ToastService,
    private readonly signalR: SignalRService
  ) {}

  ngOnInit(): void {
    this.unsubscribe = this.toastService.subscribe((messages: ToastMessage[]) => {
      this.messages = messages;
    });

    this.signalR.outbid$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event: OutbidAlertEvent | null) => {
        if (event) {
          this.toastService.show('Outbid!', event.message, 'warning', 8000);
        }
      });

    this.signalR.auctionStarted$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event: AuctionStartedEvent | null) => {
        if (event) {
          this.toastService.show('Auction Started', 'The auction has started.', 'success', 6000);
        }
      });

    this.signalR.auctionEnded$
      .pipe(takeUntil(this.destroy$))
      .subscribe((event: AuctionEndedEvent | null) => {
        if (event) {
          this.toastService.show('Auction Ended', 'The auction has ended.', 'info', 6000);
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.unsubscribe?.();
  }

  dismiss(id: string): void {
    this.toastService.dismiss(id);
  }
}
