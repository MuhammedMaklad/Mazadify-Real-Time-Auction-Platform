import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
  AbstractControl,
  ValidationErrors
} from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil, finalize } from 'rxjs/operators';
import { BidApiService } from '../../../../core/services/bid-api.service';
import { BidDto } from '../../../../core/models/bid.model';

@Component({
  selector: 'app-place-bid-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './place-bid-form.component.html',
  styleUrl: './place-bid-form.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlaceBidFormComponent implements OnInit, OnDestroy {
  @Input({ required: true }) auctionId!: string;
  @Input({ required: true }) currentBid!: number;
  @Input({ required: true }) startingPrice!: number;
  @Input() bidIncrement = 0;
  @Input() auctionEnded = false;

  /** Emits confirmed bid from server */
  @Output() bidPlaced = new EventEmitter<BidDto>();
  /** Emits amount immediately for optimistic UI */
  @Output() optimisticBid = new EventEmitter<number>();
  /** Emits when server rejects — parent should roll back optimistic row */
  @Output() optimisticRollback = new EventEmitter<void>();

  form!: FormGroup;
  submitting = false;
  serverError: string | null = null;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly fb: FormBuilder,
    private readonly bidApi: BidApiService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  get minimumBid(): number {
    return this.currentBid > 0
      ? this.currentBid + (this.bidIncrement || 0)
      : this.startingPrice;
  }

  get amountControl(): AbstractControl {
    return this.form.get('amount')!;
  }

  ngOnInit(): void {
    this.form = this.fb.group({
      amount: [
        this.minimumBid,
        [Validators.required, Validators.min(0.01), this.minimumBidValidator()]
      ]
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  submit(): void {
    if (this.form.invalid || this.submitting || this.auctionEnded) return;

    const amount: number = this.amountControl.value;
    this.serverError = null;
    this.submitting = true;

    this.optimisticBid.emit(amount);

    this.bidApi
      .placeBid(this.auctionId, amount)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.submitting = false;
          this.cdr.markForCheck();
        })
      )
      .subscribe({
        next: (bid) => {
          this.bidPlaced.emit(bid);
          this.form.reset({ amount: amount + (this.bidIncrement || 0) });
        },
        error: (err) => {
          this.optimisticRollback.emit();
          this.serverError = this.extractError(err);
        }
      });
  }

  private minimumBidValidator() {
    return (control: AbstractControl): ValidationErrors | null => {
      const value = Number(control.value);
      if (!value) return null;
      return value < this.minimumBid ? { tooLow: true } : null;
    };
  }

  private extractError(err: unknown): string {
    if (err && typeof err === 'object') {
      const e = err as Record<string, unknown>;
      if (typeof e['error'] === 'object' && e['error'] !== null) {
        const inner = e['error'] as Record<string, unknown>;
        if (typeof inner['message'] === 'string') return inner['message'];
      }
      if (typeof e['message'] === 'string') return e['message'];
    }
    return 'Failed to place bid. Please try again.';
  }
}
