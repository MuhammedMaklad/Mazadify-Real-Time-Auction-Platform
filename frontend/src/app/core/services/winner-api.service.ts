import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AuctionWinnerDto,
  DeliveryStatus,
  UpdateDeliveryStatusRequest,
  UpdateTrackingNumberRequest
} from '../models/winner.model';

@Injectable({ providedIn: 'root' })
export class WinnerApiService {
  constructor(private readonly http: HttpClient) {}

  getWinner(auctionId: string): Observable<AuctionWinnerDto> {
    return this.http.get<AuctionWinnerDto>(
      `${environment.apiUrl}/auctions/${auctionId}/winner`
    );
  }

  updateDeliveryStatus(
    auctionId: string,
    status: DeliveryStatus
  ): Observable<AuctionWinnerDto> {
    const body: UpdateDeliveryStatusRequest = { status };
    return this.http.put<AuctionWinnerDto>(
      `${environment.apiUrl}/auctions/${auctionId}/winner/delivery`,
      body
    );
  }

  updateTrackingNumber(
    auctionId: string,
    trackingNumber: string
  ): Observable<AuctionWinnerDto> {
    const body: UpdateTrackingNumberRequest = { trackingNumber };
    return this.http.put<AuctionWinnerDto>(
      `${environment.apiUrl}/auctions/${auctionId}/winner/tracking`,
      body
    );
  }

  settleAuction(auctionId: string): Observable<AuctionWinnerDto> {
    return this.http.post<AuctionWinnerDto>(
      `${environment.apiUrl}/auctions/${auctionId}/settle`,
      {}
    );
  }
}
