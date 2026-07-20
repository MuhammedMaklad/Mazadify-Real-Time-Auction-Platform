export type DeliveryStatus = 'Pending' | 'Shipped' | 'Delivered' | 'PickedUp' | 'Cancelled';
export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded';

export interface AuctionWinnerDto {
  id: string;
  auctionId: string;
  winnerId: string;
  winningBidId: string;
  paymentMethodId?: string;
  finalPrice: number;
  shippingCost: number;
  paymentStatus: PaymentStatus;
  deliveryStatus: DeliveryStatus;
  trackingNumber?: string;
  awardedAt: string;
  paidAt?: string;
}

export interface UpdateDeliveryStatusRequest {
  status: DeliveryStatus;
}

export interface UpdateTrackingNumberRequest {
  trackingNumber: string;
}
