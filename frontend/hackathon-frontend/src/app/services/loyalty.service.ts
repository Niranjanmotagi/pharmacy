import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoyaltyTransaction } from '../models/loyalty.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class LoyaltyService {
  apiUrl = `${environment.apiBaseUrl}/api/Loyalty`;

  constructor(private http: HttpClient) {}

  getPoints(): Observable<{ points: number }> {
    return this.http.get<{ points: number }>(`${this.apiUrl}/points`);
  }

  getTransactions(): Observable<LoyaltyTransaction[]> {
    return this.http.get<LoyaltyTransaction[]>(`${this.apiUrl}/transactions`);
  }
}
