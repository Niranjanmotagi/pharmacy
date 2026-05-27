import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Promotion,
  PromotionValidationResult
} from '../models/promotion.model';
import { environment } from '../../environments/environment';

/** Friendly alias re-exported so consumers can import either name. */
export type PromotionValidationResponse = PromotionValidationResult;

@Injectable({ providedIn: 'root' })
export class PromotionService {
  apiUrl = `${environment.apiBaseUrl}/api/Promotion`;

  constructor(private http: HttpClient) {}

  getActive(): Observable<Promotion[]> {
    return this.http.get<Promotion[]>(this.apiUrl);
  }

  validate(
    code: string,
    orderAmount: number
  ): Observable<PromotionValidationResult> {
    return this.http.post<PromotionValidationResult>(
      `${this.apiUrl}/validate`,
      { code, orderAmount }
    );
  }
}
