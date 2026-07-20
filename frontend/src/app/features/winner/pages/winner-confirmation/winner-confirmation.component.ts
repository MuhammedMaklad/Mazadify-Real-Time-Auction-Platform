import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { WinnerApiService } from '../../../../core/services/winner-api.service';
import { DeliveryTrackingPanelComponent } from '../../components/delivery-tracking-panel/delivery-tracking-panel.component';
import { AuctionWinnerDto } from '../../../../core/models/winner.model';

@Component({
  selector: 'app-winner-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    CurrencyPipe,
    RouterLink,
    DeliveryTrackingPanelComponent
  ],
  templateUrl: './winner-confirmation.component.html',
  styleUrl: './winner-confirmation.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class WinnerConfirmationComponent implements OnInit {
  auctionId!: string;
  winner: AuctionWinnerDto | null = null;
  loading = true;
  errorMsg: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly winnerApi: WinnerApiService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.auctionId = this.route.snapshot.paramMap.get('id') || '';
    if (this.auctionId) {
      this.fetchWinner();
    } else {
      this.errorMsg = 'Invalid auction identifier.';
      this.loading = false;
      this.cdr.markForCheck();
    }
  }

  fetchWinner(): void {
    this.loading = true;
    this.errorMsg = null;
    this.cdr.markForCheck();

    this.winnerApi.getWinner(this.auctionId).subscribe({
      next: (data) => {
        this.winner = data;
        this.loading = false;
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.loading = false;
        this.errorMsg = err.error?.message || 'Unable to retrieve winner confirmation details.';
        this.cdr.markForCheck();
      }
    });
  }

  onWinnerUpdated(updated: AuctionWinnerDto): void {
    this.winner = updated;
    this.cdr.markForCheck();
  }

  get totalDue(): number {
    if (!this.winner) return 0;
    return (this.winner.finalPrice || 0) + (this.winner.shippingCost || 0);
  }

  proceedToPayment(): void {
    this.router.navigate(['/payment-methods']);
  }
}
