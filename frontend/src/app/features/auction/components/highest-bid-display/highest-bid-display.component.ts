import {
  Component,
  Input,
  OnChanges,
  SimpleChanges,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';

@Component({
  selector: 'app-highest-bid-display',
  standalone: true,
  imports: [CommonModule, CurrencyPipe],
  templateUrl: './highest-bid-display.component.html',
  styleUrl: './highest-bid-display.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class HighestBidDisplayComponent implements OnChanges {
  /** Current highest bid — updated by parent when SignalR fires */
  @Input({ required: true }) currentBid!: number;
  /** Shown when currentBid is 0 */
  @Input({ required: true }) startingPrice!: number;
  /** Used to calculate minimum next bid */
  @Input() bidIncrement = 0;

  pulsing = false;
  private pulseTimer: ReturnType<typeof setTimeout> | null = null;

  constructor(private readonly cdr: ChangeDetectorRef) {}

  ngOnChanges(changes: SimpleChanges): void {
    const bidChange = changes['currentBid'];
    if (
      bidChange &&
      !bidChange.isFirstChange() &&
      bidChange.currentValue > bidChange.previousValue
    ) {
      this.triggerPulse();
    }
  }

  get displayAmount(): number {
    return this.currentBid > 0 ? this.currentBid : this.startingPrice;
  }

  get minimumNextBid(): number {
    return this.currentBid > 0
      ? this.currentBid + this.bidIncrement
      : this.startingPrice;
  }

  private triggerPulse(): void {
    if (this.pulseTimer) {
      clearTimeout(this.pulseTimer);
      this.pulsing = false;
      this.cdr.markForCheck();
    }
    setTimeout(() => {
      this.pulsing = true;
      this.cdr.markForCheck();
      this.pulseTimer = setTimeout(() => {
        this.pulsing = false;
        this.pulseTimer = null;
        this.cdr.markForCheck();
      }, 550);
    }, 10);
  }
}
