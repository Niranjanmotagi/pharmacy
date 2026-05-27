import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Medicine } from '../models/medicine.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class MedicineService {
  apiUrl = `${environment.apiBaseUrl}/api/Medicine`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Medicine[]> {
    return this.http.get<Medicine[]>(this.apiUrl);
  }

  getById(id: number): Observable<Medicine> {
    return this.http.get<Medicine>(`${this.apiUrl}/${id}`);
  }

  add(med: Partial<Medicine>): Observable<Medicine> {
    return this.http.post<Medicine>(this.apiUrl, med);
  }

  update(id: number, med: Partial<Medicine>): Observable<Medicine> {
    return this.http.put<Medicine>(`${this.apiUrl}/${id}`, med);
  }

  delete(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.apiUrl}/${id}`);
  }
}
