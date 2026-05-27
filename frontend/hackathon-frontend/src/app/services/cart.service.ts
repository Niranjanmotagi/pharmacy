import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CartItem } from '../models/cart-item.model';
import { environment } from '../../environments/environment';

export interface CartTotal {
  totalQuantity: number;
  totalPrice: number;
}

export interface CartMessageResponse {
  message: string;
}

@Injectable({ providedIn: 'root' })
export class CartService {
  apiUrl = `${environment.apiBaseUrl}/api/Cart`;

  constructor(private http: HttpClient) {}

  getMyCart(): Observable<CartItem[]> {
    return this.http.get<CartItem[]>(this.apiUrl);
  }

  getTotal(): Observable<CartTotal> {
    return this.http.get<CartTotal>(`${this.apiUrl}/total`);
  }

  addToCart(medicineId: number, quantity = 1): Observable<CartMessageResponse> {
    return this.http.post<CartMessageResponse>(this.apiUrl, {
      medicineId,
      quantity
    });
  }

  updateQuantity(id: number, quantity: number): Observable<CartMessageResponse> {
    return this.http.put<CartMessageResponse>(`${this.apiUrl}/${id}`, {
      quantity
    });
  }

  remove(id: number): Observable<CartMessageResponse> {
    return this.http.delete<CartMessageResponse>(`${this.apiUrl}/${id}`);
  }
}
