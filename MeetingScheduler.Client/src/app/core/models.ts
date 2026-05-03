export interface Room {
  id: string;
  name: string;
  floor: string;
  location: string;
  capacity: number;
  amenities?: string | null;
  exchangeEmail?: string | null;
  isActive: boolean;
}

export interface UpsertRoomRequest {
  name: string;
  floor: string;
  location: string;
  capacity: number;
  amenities?: string | null;
  exchangeEmail?: string | null;
  isActive: boolean;
}

export interface BookingInstance {
  id: string;
  seriesId?: string | null;
  roomId: string;
  roomName: string;
  subject: string;
  organizerEmail: string;
  attendees: string[];
  startAt: string;
  endAt: string;
  isRecurring: boolean;
}

export interface CreateBookingRequest {
  roomId: string;
  subject: string;
  attendees: string[];
  startAt: string;
  endAt: string;
  timeZone: string;
  recurrence?: {
    type: 'None' | 'Daily' | 'Weekly' | 'Monthly';
    interval: number;
    until?: string | null;
  } | null;
}

export interface DashboardStats {
  totalRooms: number;
  bookingsToday: number;
  adminConsentGranted: boolean;
  graphSyncActive: boolean;
}

export interface Profile {
  displayName: string;
  email: string;
  tenantId: string;
  roles: string[];
}
