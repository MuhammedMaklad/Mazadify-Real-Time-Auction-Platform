import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { BidDto, PagedResult, PlaceBidRequest } from '../models/bid.model';

@Injectable({ providedIn: 'root' })
export class BidApiService {
  constructor(private readonly http: HttpClient) {}

  /**
   * POST /api/auctions/{auctionId}/bids
   * Sends a bid with an auto-generated idempotency key.
   */
  placeBid(auctionId: string, amount: number): Observable<BidDto> {
    const body: PlaceBidRequest = {
      amount,
      idempotencyKey: crypto.randomUUID()
    };
    return this.http.post<BidDto>(
      `${environment.apiUrl}/auctions/${auctionId}/bids`,
      body
    );
  }

  /**
   * GET /api/auctions/{auctionId}/bids
   * Returns paginated bid history for an auction.
   */
  getBidHistory(
    auctionId: string,
    page = 1,
    pageSize = 20
  ): Observable<PagedResult<BidDto>> {
    const params = new HttpParams()
      .set('page', page)
      .set('pageSize', pageSize);

    return this.http.get<PagedResult<BidDto>>(
      `${environment.apiUrl}/auctions/${auctionId}/bids`,
      { params }
    );
  }
}
