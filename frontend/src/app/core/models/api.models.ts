export interface UserInfo {
  id: string;
  username: string;
  email: string;
  roles: string[];
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiry: string;
  user: UserInfo;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  role: string;
}

export interface PaymentMethodDto {
  id: string;
  type: string;
  provider: string;
  lastFour: string | null;
  expiryMonth: string | null;
  expiryYear: string | null;
  isDefault: boolean;
  isActive: boolean;
  createdAt: string;
}

export interface AddPaymentMethodRequest {
  type: string;
  provider: string;
  token: string;
  lastFour?: string;
  expiryMonth?: string;
  expiryYear?: string;
  isDefault: boolean;
}
