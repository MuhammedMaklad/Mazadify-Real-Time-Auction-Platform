import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notification } from '../models/notification.model';

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly baseUrl = `${environment.apiUrl}/notifications`;

  constructor(private readonly http: HttpClient) {}

  getNotifications(
    unreadOnly = false,
    page = 1,
    pageSize = 20
  ): Observable<PagedResult<Notification>> {
    const params = new HttpParams()
      .set('unreadOnly', unreadOnly)
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResult<Notification>>(this.baseUrl, { params });
  }

  markAsRead(id: string): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}/read`, {});
  }

  markAllAsRead(): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/read-all`, {});
  }
}
