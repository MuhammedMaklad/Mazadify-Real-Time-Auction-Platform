import {
  Component,
  Input,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil, filter } from 'rxjs/operators';
import { SignalRService } from '../../../../core/services/signalr.service';

@Component({
  selector: 'app-countdown',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="countdown" [class.ending]="isEnding">
      <span class="time">{{ formattedTime }}</span>
    </div>
  `,
  styles: [`
    .countdown { font-variant-numeric: tabular-nums; }
    .ending { color: #d32f2f; font-weight: 700; }
    .time { font-size: 1.25rem; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CountdownComponent implements OnInit, OnDestroy {
  @Input({ required: true }) auctionId!: string;
  @Input({ required: true }) endTime!: string;

  formattedTime = '00:00:00';
  isEnding = false;

  private endDate!: Date;
  private timer: any;
  private destroy$ = new Subject<void>();
  private serverDriftMs = 0;

  constructor(private readonly signalR: SignalRService) {}

  ngOnInit(): void {
    this.endDate = new Date(this.endTime);
    this.tick();
    this.timer = setInterval(() => this.tick(), 1000);

    this.signalR.countdownSync$
      .pipe(
        takeUntil(this.destroy$),
        filter((sync) => sync?.auctionId === this.auctionId)
      )
      .subscribe((sync) => {
        if (sync) {
          const serverTime = new Date(sync.serverTimeUtc).getTime();
          this.serverDriftMs = serverTime - Date.now();
        }
      });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    clearInterval(this.timer);
  }

  private tick(): void {
    const remaining = this.endDate.getTime() - (Date.now() + this.serverDriftMs);

    if (remaining <= 0) {
      this.formattedTime = '00:00:00';
      this.isEnding = false;
      return;
    }

    this.isEnding = remaining < 60_000;

    const totalSeconds = Math.floor(remaining / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;

    this.formattedTime = `${this.pad(hours)}:${this.pad(minutes)}:${this.pad(seconds)}`;
  }

  private pad(value: number): string {
    return value.toString().padStart(2, '0');
  }
}
