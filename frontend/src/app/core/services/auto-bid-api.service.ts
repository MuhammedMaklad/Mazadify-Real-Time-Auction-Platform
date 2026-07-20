import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  AutoBidDto,
  CreateAutoBidRequest,
  UpdateAutoBidRequest
} from '../models/auto-bid.model';

@Injectable({ providedIn: 'root' })
export class AutoBidApiService {
  constructor(private readonly http: HttpClient) {}

  getMyAutoBid(auctionId: string): Observable<AutoBidDto> {
    return this.http.get<AutoBidDto>(
      `${environment.apiUrl}/auctions/${auctionId}/auto-bids/me`
    );
  }

  createAutoBid(auctionId: string, maxAmount: number): Observable<AutoBidDto> {
    const body: CreateAutoBidRequest = { maxAmount };
    return this.http.post<AutoBidDto>(
      `${environment.apiUrl}/auctions/${auctionId}/auto-bids`,
      body
    );
  }

  updateAutoBid(
    auctionId: string,
    id: string,
    request: UpdateAutoBidRequest
  ): Observable<AutoBidDto> {
    return this.http.put<AutoBidDto>(
      `${environment.apiUrl}/auctions/${auctionId}/auto-bids/${id}`,
      request
    );
  }

  deleteAutoBid(auctionId: string, id: string): Observable<void> {
    return this.http.delete<void>(
      `${environment.apiUrl}/auctions/${auctionId}/auto-bids/${id}`
    );
  }
}
