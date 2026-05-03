import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { BookingInstance, CreateBookingRequest, DashboardStats, Profile, Room, UpsertRoomRequest } from './models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly baseUrl = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  me(): Observable<Profile> {
    return this.http.get<Profile>(`${this.baseUrl}/profile/me`);
  }

  stats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.baseUrl}/dashboard/stats`);
  }

  rooms(): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.baseUrl}/rooms`);
  }

  createRoom(request: UpsertRoomRequest): Observable<Room> {
    return this.http.post<Room>(`${this.baseUrl}/rooms`, request);
  }

  updateRoom(id: string, request: UpsertRoomRequest): Observable<Room> {
    return this.http.put<Room>(`${this.baseUrl}/rooms/${id}`, request);
  }

  deleteRoom(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/rooms/${id}`);
  }

  bookings(start: Date, end: Date): Observable<BookingInstance[]> {
    const params = new HttpParams()
      .set('start', start.toISOString())
      .set('end', end.toISOString());

    return this.http.get<BookingInstance[]>(`${this.baseUrl}/bookings`, { params });
  }

  createBooking(request: CreateBookingRequest): Observable<{ seriesId: string; instances: BookingInstance[] }> {
    return this.http.post<{ seriesId: string; instances: BookingInstance[] }>(`${this.baseUrl}/bookings`, request);
  }

  saveAdminConsent(request: { microsoftTenantId: string; organizationName: string; customDomain?: string | null }): Observable<unknown> {
    return this.http.post(`${this.baseUrl}/tenants/admin-consent`, request);
  }
}
