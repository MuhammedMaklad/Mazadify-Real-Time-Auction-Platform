import {
  Component,
  Input,
  Output,
  EventEmitter,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { WinnerApiService } from '../../../../core/services/winner-api.service';
import { ToastService } from '../../../../core/services/toast.service';
import {
  AuctionWinnerDto,
  DeliveryStatus
} from '../../../../core/models/winner.model';

@Component({
  selector: 'app-delivery-tracking-panel',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './delivery-tracking-panel.component.html',
  styleUrl: './delivery-tracking-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DeliveryTrackingPanelComponent {
  @Input({ required: true }) auctionId!: string;
  @Input({ required: true }) winner!: AuctionWinnerDto;
  @Input() isAdminOrSeller = false;

  @Output() winnerUpdated = new EventEmitter<AuctionWinnerDto>();

  updatingStatus = false;
  updatingTracking = false;
  selectedStatus: DeliveryStatus = 'Shipped';
  trackingInput = '';
  copied = false;

  readonly steps: DeliveryStatus[] = ['Pending', 'Shipped', 'Delivered'];

  constructor(
    private readonly winnerApi: WinnerApiService,
    private readonly toast: ToastService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  get currentStepIndex(): number {
    switch (this.winner.deliveryStatus) {
      case 'Pending':
        return 0;
      case 'Shipped':
        return 1;
      case 'Delivered':
      case 'PickedUp':
        return 2;
      default:
        return -1;
    }
  }

  isStepCompleted(index: number): boolean {
    return this.currentStepIndex >= index;
  }

  copyTracking(): void {
    if (!this.winner.trackingNumber) return;
    navigator.clipboard.writeText(this.winner.trackingNumber);
    this.copied = true;
    this.toast.showSuccess('Tracking number copied!');
    this.cdr.markForCheck();
    setTimeout(() => {
      this.copied = false;
      this.cdr.markForCheck();
    }, 2000);
  }

  updateStatus(): void {
    if (!this.selectedStatus) return;

    this.updatingStatus = true;
    this.cdr.markForCheck();

    this.winnerApi
      .updateDeliveryStatus(this.auctionId, this.selectedStatus)
      .subscribe({
        next: (updated) => {
          this.winner = updated;
          this.winnerUpdated.emit(updated);
          this.updatingStatus = false;
          this.toast.showSuccess(`Delivery status updated to ${updated.deliveryStatus}`);
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.updatingStatus = false;
          this.toast.showError(err.error?.message || 'Failed to update delivery status.');
          this.cdr.markForCheck();
        }
      });
  }

  saveTracking(): void {
    if (!this.trackingInput.trim()) return;

    this.updatingTracking = true;
    this.cdr.markForCheck();

    this.winnerApi
      .updateTrackingNumber(this.auctionId, this.trackingInput.trim())
      .subscribe({
        next: (updated) => {
          this.winner = updated;
          this.winnerUpdated.emit(updated);
          this.updatingTracking = false;
          this.trackingInput = '';
          this.toast.showSuccess('Tracking number updated successfully!');
          this.cdr.markForCheck();
        },
        error: (err) => {
          this.updatingTracking = false;
          this.toast.showError(err.error?.message || 'Failed to update tracking number.');
          this.cdr.markForCheck();
        }
      });
  }
}
