import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

export interface LoginResponse {
  token: string;
  username: string;
  role: string;
  userId: number;
}

export interface RegisterResponse {
  message: string;
  userId: number;
}

export interface RegisterPayload {
  username: string;
  password: string;
  role?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  phoneNumber?: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  apiUrl = `${environment.apiBaseUrl}/api/Auth`;

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(`${this.apiUrl}/login`, { username, password })
      .pipe(
        tap((res: LoginResponse) => {
          if (typeof window !== 'undefined') {
            localStorage.setItem('token', res.token);
            localStorage.setItem('username', res.username);
            localStorage.setItem('role', res.role);
            localStorage.setItem('userId', String(res.userId));
          }
        })
      );
  }

  register(payload: RegisterPayload): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, {
      role: 'Customer',
      ...payload
    });
  }

  logout(): void {
    if (typeof window !== 'undefined') {
      localStorage.removeItem('token');
      localStorage.removeItem('username');
      localStorage.removeItem('role');
      localStorage.removeItem('userId');
    }
  }

  getRole(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('role');
  }

  getUsername(): string | null {
    if (typeof window === 'undefined') return null;
    return localStorage.getItem('username');
  }

  isLoggedIn(): boolean {
    if (typeof window === 'undefined') return false;
    return !!localStorage.getItem('token');
  }

  isAdmin(): boolean {
    return this.getRole() === 'Admin';
  }
}
