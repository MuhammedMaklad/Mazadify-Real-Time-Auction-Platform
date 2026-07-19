import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class AuthTokenService {
  private readonly key = 'mazadify_access_token';

  getToken(): string | null {
    return typeof window !== 'undefined' ? localStorage.getItem(this.key) : null;
  }

  setToken(token: string): void {
    if (typeof window !== 'undefined') {
      localStorage.setItem(this.key, token);
    }
  }

  clearToken(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem(this.key);
    }
  }
}
