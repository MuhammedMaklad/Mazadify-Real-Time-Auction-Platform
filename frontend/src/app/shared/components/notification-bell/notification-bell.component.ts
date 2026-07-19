import {
  Component,
  OnInit,
  OnDestroy,
  ChangeDetectionStrategy,
  ChangeDetectorRef
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { NotificationApiService } from '../../../core/services/notification-api.service';
import { Notification } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="bell-wrapper">
      <button type="button" class="bell-button" (click)="toggle()" aria-label="Notifications">
        <span class="bell-icon">??</span>
        <span *ngIf="unreadCount > 0" class="badge">{{ unreadCount }}</span>
      </button>

      <div *ngIf="open" class="dropdown">
        <div class="dropdown-header">
          <span>Notifications</span>
          <button *ngIf="unreadCount > 0" type="button" (click)="markAllRead()">Mark all read</button>
        </div>
        <ul class="notification-list">
          <li
            *ngFor="let n of notifications; trackBy: trackById"
            class="notification-item"
            [class.unread]="!n.isRead"
            (click)="markRead(n)">
            <div class="title">{{ n.title }}</div>
            <div class="message">{{ n.message }}</div>
            <div class="time">{{ n.createdAt | date:'short' }}</div>
          </li>
          <li *ngIf="notifications.length === 0" class="empty">No notifications</li>
        </ul>
      </div>
    </div>
  `,
  styles: [`
    .bell-wrapper { position: relative; display: inline-block; }
    .bell-button { background: none; border: none; font-size: 1.5rem; cursor: pointer; position: relative; }
    .badge { position: absolute; top: -4px; right: -4px; background: #d32f2f; color: white; border-radius: 50%; width: 20px; height: 20px; font-size: 0.75rem; display: grid; place-items: center; }
    .dropdown { position: absolute; top: 120%; right: 0; width: 320px; max-height: 400px; background: white; border: 1px solid #e0e0e0; border-radius: 8px; box-shadow: 0 4px 12px rgba(0,0,0,0.15); overflow: hidden; z-index: 100; }
    .dropdown-header { display: flex; justify-content: space-between; align-items: center; padding: 0.75rem 1rem; border-bottom: 1px solid #e0e0e0; font-weight: 600; }
    .dropdown-header button { background: none; border: none; color: #1976d2; cursor: pointer; font-size: 0.875rem; }
    .notification-list { list-style: none; margin: 0; padding: 0; overflow-y: auto; max-height: 330px; }
    .notification-item { padding: 0.75rem 1rem; border-bottom: 1px solid #f0f0f0; cursor: pointer; }
    .notification-item.unread { background: #e3f2fd; }
    .notification-item:hover { background: #f5f5f5; }
    .title { font-weight: 600; font-size: 0.95rem; }
    .message { color: #616161; font-size: 0.875rem; }
    .time { color: #9e9e9e; font-size: 0.75rem; margin-top: 0.25rem; }
    .empty { padding: 1rem; color: #9e9e9e; text-align: center; }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  notifications: Notification[] = [];
  unreadCount = 0;
  open = false;

  private destroy$ = new Subject<void>();

  constructor(
    private readonly api: NotificationApiService,
    private readonly cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggle(): void {
    this.open = !this.open;
    if (this.open) this.load();
  }

  markRead(notification: Notification): void {
    if (notification.isRead) return;

    this.api
      .markAsRead(notification.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.load());
  }

  markAllRead(): void {
    this.api
      .markAllAsRead()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.load());
  }

  trackById(index: number, item: Notification): string {
    return item.id;
  }

  private load(): void {
    this.api
      .getNotifications(true, 1, 50)
      .pipe(takeUntil(this.destroy$))
      .subscribe((result: { items: Notification[]; totalCount: number }) => {
        this.notifications = result.items;
        this.unreadCount = result.items.filter((n: Notification) => !n.isRead).length;
        this.cdr.markForCheck();
      });
  }
}
