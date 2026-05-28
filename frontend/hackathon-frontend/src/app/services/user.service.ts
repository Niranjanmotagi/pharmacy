import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  ChangePasswordPayload,
  UpdateProfilePayload,
  UserProfile
} from '../models/user-profile.model';

@Injectable({ providedIn: 'root' })
export class UserService {
  apiUrl = `${environment.apiBaseUrl}/api/User`;

  constructor(private http: HttpClient) {}

  me(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/me`);
  }

  updateProfile(
    payload: UpdateProfilePayload
  ): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.apiUrl}/me`, payload);
  }

  changePassword(
    payload: ChangePasswordPayload
  ): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(
      `${this.apiUrl}/me/password`,
      payload
    );
  }
}
