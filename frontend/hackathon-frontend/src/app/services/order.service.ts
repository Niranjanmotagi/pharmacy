import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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

  myOrders() {
    return this.http.get<Order[]>(`${this.apiUrl}/my`);
  }

  allOrders() {
    return this.http.get<Order[]>(`${this.apiUrl}/all`);
  }

  /** Legacy: change status by raw string. Kept for backward-compat. */
  updateStatus(id: number, status: string) {
    return this.http.put<Order>(`${this.apiUrl}/${id}/status`, JSON.stringify(status), {
      headers: { 'Content-Type': 'application/json' }
    });
  }

  approve(id: number, estimatedDeliveryDate?: string | Date | null) {
    return this.http.put<{
      message: string;
      orderId: number;
      orderNumber: string;
      status: string;
      estimatedDeliveryDate: string;
    }>(`${this.apiUrl}/${id}/approve`, { estimatedDeliveryDate });
  }

  reject(id: number, reason: string) {
    return this.http.put<{
      message: string;
      orderId: number;
      orderNumber: string;
      status: string;
      rejectionReason: string;
    }>(`${this.apiUrl}/${id}/reject`, { reason });
  }

  deliver(id: number) {
    return this.http.put<{
      message: string;
      orderId: number;
      status: string;
    }>(`${this.apiUrl}/${id}/deliver`, {});
  }
}
