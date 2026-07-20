export interface UserBrief {
  id: string;
  username: string;
}

export interface BidDto {
  id: string;
  auctionId: string;
  bidderId: string;
  amount: number;
  /** 'Active' | 'Outbid' | 'Won' */
  status: string;
  placedAt: string;
  isAutoBid: boolean;
  bidder: UserBrief | null;
}

export interface PlaceBidRequest {
  amount: number;
  idempotencyKey: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/** Optimistic bid shown in the UI before server confirms */
export interface PendingBid {
  tempId: string;
  amount: number;
  placedAt: string;
  pending: true;
}

export type BidRow = (BidDto & { pending?: false }) | (PendingBid & { pending: true });
