import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Order } from '../models/order.model';
import { environment } from '../../environments/environment';

export interface PlaceOrderResponse {
  message: string;
  orderId: number;
  orderNumber: string;
  totalAmount: number;
  discountAmount: number;
  finalAmount: number;
  estimatedDeliveryDate?: string | null;
  pointsEarned: number;
}

export interface PlaceOrderPayload {
  prescription?: File | null;
  promoCode?: string;
  address?: string;
  phone?: string;
  notes?: string;
}

export interface OrderActionResponse {
  message: string;
  orderId: number;
  status: string;
}

@Injectable({ providedIn: 'root' })
export class OrderService {
  apiUrl = `${environment.apiBaseUrl}/api/Order`;

  constructor(private http: HttpClient) {}

  placeOrder(payload: PlaceOrderPayload) {
    const form = new FormData();
    if (payload.prescription) form.append('prescription', payload.prescription);
    if (payload.promoCode) form.append('promoCode', payload.promoCode);
    if (payload.address) form.append('address', payload.address);
    if (payload.phone) form.append('phone', payload.phone);
    if (payload.notes) form.append('notes', payload.notes);

    return this.http.post<PlaceOrderResponse>(`${this.apiUrl}/place`, form);
  }

  myOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/my`);
  }

  allOrders(): Observable<Order[]> {
    return this.http.get<Order[]>(`${this.apiUrl}/all`);
  }

  /** Legacy: change status by raw string. */
  updateStatus(id: number, status: string): Observable<Order> {
    return this.http.put<Order>(
      `${this.apiUrl}/${id}/status`,
      JSON.stringify(status),
      { headers: { 'Content-Type': 'application/json' } }
    );
  }

  approve(
    id: number,
    estimatedDeliveryDate?: string | Date | null
  ): Observable<OrderActionResponse & { orderNumber: string; estimatedDeliveryDate: string }> {
    return this.http.put<OrderActionResponse & { orderNumber: string; estimatedDeliveryDate: string }>(
      `${this.apiUrl}/${id}/approve`,
      { estimatedDeliveryDate }
    );
  }

  reject(id: number, reason: string): Observable<OrderActionResponse & { orderNumber: string; rejectionReason: string }> {
    return this.http.put<OrderActionResponse & { orderNumber: string; rejectionReason: string }>(
      `${this.apiUrl}/${id}/reject`,
      { reason }
    );
  }

  pack(id: number): Observable<OrderActionResponse> {
    return this.http.put<OrderActionResponse>(`${this.apiUrl}/${id}/pack`, {});
  }

  ship(id: number): Observable<OrderActionResponse> {
    return this.http.put<OrderActionResponse>(`${this.apiUrl}/${id}/ship`, {});
  }

  deliver(id: number): Observable<OrderActionResponse> {
    return this.http.put<OrderActionResponse>(`${this.apiUrl}/${id}/deliver`, {});
  }

  /**
   * Admin-only authenticated download of the prescription file. Returns a
   * Blob so we can build a local object URL for image/PDF preview.
   */
  getPrescriptionBlob(id: number): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/prescription`, {
      responseType: 'blob'
    });
  }
}
