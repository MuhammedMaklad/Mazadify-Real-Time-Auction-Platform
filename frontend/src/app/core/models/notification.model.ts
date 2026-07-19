export interface Notification {
  id: string;
  type: string;
  title: string;
  message: string;
  payload?: string;
  isRead: boolean;
  createdAt: string;
  readAt?: string;
}

export interface BidPlacedEvent {
  auctionId: string;
  bidId: string;
  bidderId: string;
  bidderUsername: string;
  amount: number;
  placedAt: string;
}

export interface AuctionEvent {
  auctionId: string;
  occurredAt: string;
}

export interface AuctionStartedEvent extends AuctionEvent { }

export interface AuctionEndedEvent extends AuctionEvent {
  winnerId?: string;
  winningAmount?: number;
}

export interface OutbidAlertEvent {
  auctionId: string;
  bidId: string;
  newAmount: number;
  message: string;
}

export interface CountdownSync {
  auctionId: string;
  serverTimeUtc: string;
}
