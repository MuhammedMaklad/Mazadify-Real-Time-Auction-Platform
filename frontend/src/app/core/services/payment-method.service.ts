import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AddPaymentMethodRequest, PaymentMethodDto } from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PaymentMethodService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'http://localhost:5000/api/payment-methods';

  getAll(): Observable<PaymentMethodDto[]> {
    return this.http.get<PaymentMethodDto[]>(this.apiUrl, { withCredentials: true });
  }

  add(request: AddPaymentMethodRequest): Observable<PaymentMethodDto> {
    return this.http.post<PaymentMethodDto>(this.apiUrl, request, { withCredentials: true });
  }

  setDefault(id: string): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${id}/set-default`, {}, { withCredentials: true });
  }

  deactivate(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { withCredentials: true });
  }
}
