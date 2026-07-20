export interface AutoBidDto {
  id: string;
  auctionId: string;
  maxAmount: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateAutoBidRequest {
  maxAmount: number;
}

export interface UpdateAutoBidRequest {
  maxAmount: number;
  isActive: boolean;
}
