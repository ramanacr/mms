import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BookingInstance, CreateBookingRequest, DashboardStats, Profile, Room, UpdateBookingRequest, UpsertRoomRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  me(): Observable<Profile> {
    return this.http.get<Profile>(`${this.baseUrl}/profile/me`, this.devOptions());
  }

  stats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/dashboard/stats`, this.devOptions());
  }

  rooms(): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.baseUrl}/rooms`, this.devOptions());
  }

  createRoom(request: UpsertRoomRequest): Observable<Room> {
    return this.http.post<Room>(`${this.baseUrl}/rooms`, request, this.devOptions());
  }

  updateRoom(id: string, request: UpsertRoomRequest): Observable<Room> {
    return this.http.put<Room>(`${this.baseUrl}/rooms/${id}`, request, this.devOptions());
  }

  deleteRoom(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/rooms/${id}`, this.devOptions());
  }

  bookings(start: Date, end: Date): Observable<BookingInstance[]> {
    const params = new HttpParams()
      .set('start', start.toISOString())
      .set('end', end.toISOString());

    return this.http.get<BookingInstance[]>(`${this.baseUrl}/bookings`, { ...this.devOptions(), params });
  }

  createBooking(request: CreateBookingRequest): Observable<{ seriesId: string; instances: BookingInstance[] }> {
    return this.http.post<{ seriesId: string; instances: BookingInstance[] }>(`${this.baseUrl}/bookings`, request, this.devOptions());
  }

  updateBooking(id: string, request: UpdateBookingRequest): Observable<BookingInstance> {
    return this.http.put<BookingInstance>(`${this.baseUrl}/bookings/${id}`, request, this.devOptions());
  }

  startAdminConsent(): Observable<{ consentUrl: string }> {
    return this.http.post<{ consentUrl: string }>(`${this.baseUrl}/tenants/admin-consent/start`, {}, this.devOptions());
  }

  completeAdminConsent(request: { microsoftTenantId: string; adminConsentGranted: boolean; state: string }): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/tenants/admin-consent`, request, this.devOptions());
  }

  private devOptions(): { headers?: HttpHeaders } {
    if (environment.production) {
      return {};
    }

    const role = localStorage.getItem('devRole') === 'user' ? 'user' : 'admin';
    return { headers: new HttpHeaders({ 'X-Dev-Role': role }) };
  }
}
