import {
  Component,
  Input,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AutoBidApiService } from '../../../../core/services/auto-bid-api.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AutoBidDto } from '../../../../core/models/auto-bid.model';

@Component({
  selector: 'app-auto-bid-panel',
  standalone: true,
  imports: [CommonModule, FormsModule, CurrencyPipe],
  templateUrl: './auto-bid-panel.component.html',
  styleUrl: './auto-bid-panel.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AutoBidPanelComponent implements OnInit {
  @Input({ required: true }) auctionId!: string;
  @Input({ required: true }) currentHighestBid!: number;
  @Input({ required: true }) bidIncrement!: number;

  autoBid: AutoBidDto | null = null;
  loading = true;
  saving = false;
  maxAmountInput: number | null = null;
  errorMsg: string | null = null;

  constructor(
    private readonly autoBidApi: AutoBidApiService,
    private readonly toast: ToastService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.fetchAutoBid();
  }

  fetchAutoBid(): void {
    this.loading = true;
    this.errorMsg = null;
    this.cdr.markForCheck();

    this.autoBidApi.getMyAutoBid(this.auctionId).subscribe({
      next: (dto) => {
        this.autoBid = dto;
        this.maxAmountInput = dto.maxAmount;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: () => {
        this.autoBid = null;
        this.loading = false;
        this.cdr.markForCheck();
      }
    });
  }

  get minAllowedAmount(): number {
    return (this.currentHighestBid || 0) + (this.bidIncrement || 1);
  }

  saveAutoBid(): void {
    if (!this.maxAmountInput || this.maxAmountInput < this.minAllowedAmount) {
      this.errorMsg = `Max amount must be at least ${this.minAllowedAmount}.`;
      this.cdr.markForCheck();
      return;
    }

    this.saving = true;
    this.errorMsg = null;
    this.cdr.markForCheck();

    if (this.autoBid) {
      // Update
      this.autoBidApi
        .updateAutoBid(this.auctionId, this.autoBid.id, {
          maxAmount: this.maxAmountInput,
          isActive: true
        })
        .subscribe({
          next: (updated) => {
            this.autoBid = updated;
            this.saving = false;
            this.toast.showSuccess('Auto-bid limit updated successfully!');
            this.cdr.markForCheck();
          },
          error: (err) => {
            this.saving = false;
            this.errorMsg = err.error?.message || 'Failed to update auto-bid.';
            this.toast.showError(this.errorMsg || 'Error updating auto-bid.');
            this.cdr.markForCheck();
          }
        });
    } else {
      // Create
      this.autoBidApi
        .createAutoBid(this.auctionId, this.maxAmountInput)
        .subscribe({
          next: (created) => {
            this.autoBid = created;
            this.saving = false;
            this.toast.showSuccess('Auto-bid setup successfully!');
            this.cdr.markForCheck();
          },
          error: (err) => {
            this.saving = false;
            this.errorMsg = err.error?.message || 'Failed to set auto-bid.';
            this.toast.showError(this.errorMsg || 'Error setting auto-bid.');
            this.cdr.markForCheck();
          }
        });
    }
  }

  toggleActiveState(): void {
    if (!this.autoBid) return;

    this.saving = true;
    this.cdr.markForCheck();

    const newActiveState = !this.autoBid.isActive;
    this.autoBidApi
      .updateAutoBid(this.auctionId, this.autoBid.id, {
        maxAmount: this.autoBid.maxAmount,
        isActive: newActiveState
      })
      .subscribe({
        next: (updated) => {
          this.autoBid = updated;
          this.saving = false;
          this.toast.showSuccess(
            newActiveState ? 'Auto-bid activated!' : 'Auto-bid paused.'
          );
          this.cdr.markForCheck();
        },
        error: () => {
          this.saving = false;
          this.toast.showError('Failed to toggle auto-bid status.');
          this.cdr.markForCheck();
        }
      });
  }

  removeAutoBid(): void {
    if (!this.autoBid) return;

    this.saving = true;
    this.cdr.markForCheck();

    this.autoBidApi.deleteAutoBid(this.auctionId, this.autoBid.id).subscribe({
      next: () => {
        this.autoBid = null;
        this.maxAmountInput = null;
        this.saving = false;
        this.toast.showSuccess('Auto-bid removed.');
        this.cdr.markForCheck();
      },
      error: () => {
        this.saving = false;
        this.toast.showError('Failed to remove auto-bid.');
        this.cdr.markForCheck();
      }
    });
  }
}
