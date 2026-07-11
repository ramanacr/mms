import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DatePickerModule } from 'primeng/datepicker';
import { DialogModule } from 'primeng/dialog';
import { DividerModule } from 'primeng/divider';
import { DrawerModule } from 'primeng/drawer';
import { FluidModule } from 'primeng/fluid';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { TabsModule } from 'primeng/tabs';
import { TagModule } from 'primeng/tag';
import { TextareaModule } from 'primeng/textarea';
import { ToggleSwitchModule } from 'primeng/toggleswitch';
import { ApiService } from '../../core/api.service';
import { AuthService } from '../../core/auth.service';
import { BookingFormValue, buildCreateBookingRequest } from '../../core/booking.mapper';
import { BookingInstance, DashboardStats, Profile, Room, UpsertRoomRequest } from '../../core/models';
import { environment } from '../../../environments/environment';

export const DEFAULT_PROFILE: Profile = { displayName: 'Scheduler User', email: '', tenantId: '', roles: [] };
export type DevelopmentRole = 'admin' | 'user';
export const DEFAULT_TIME_ZONE = Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC';

export function resolveDevelopmentRole(pathname: string, storedRole: string | null): DevelopmentRole {
  if (pathname.includes('/dev-user')) {
    return 'user';
  }

  if (pathname.includes('/dev-admin')) {
    return 'admin';
  }

  return storedRole === 'user' ? 'user' : 'admin';
}

export function buildTimeZoneOptions(currentTimeZone = DEFAULT_TIME_ZONE): { label: string; value: string }[] {
  const supportedValuesOf = (Intl as typeof Intl & { supportedValuesOf?: (key: 'timeZone') => string[] }).supportedValuesOf;
  const browserZones = supportedValuesOf?.('timeZone') ?? [];
  const preferredZones = [
    'UTC',
    'Asia/Calcutta',
    'Asia/Dubai',
    'Europe/London',
    'Europe/Paris',
    'America/New_York',
    'America/Chicago',
    'America/Los_Angeles'
  ];
  const zones = [currentTimeZone, ...preferredZones, ...browserZones]
    .filter(Boolean)
    .filter((zone, index, values) => values.indexOf(zone) === index)
    .sort((a, b) => a.localeCompare(b));

  return zones.map((zone) => ({ label: zone.replaceAll('_', ' '), value: zone }));
}

@Component({
  standalone: true,
  selector: 'app-scheduler-shell',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    AutoCompleteModule,
    ButtonModule,
    CardModule,
    DatePickerModule,
    DialogModule,
    DividerModule,
    DrawerModule,
    FluidModule,
    FullCalendarModule,
    InputNumberModule,
    InputTextModule,
    SelectModule,
    TableModule,
    TabsModule,
    TagModule,
    TextareaModule,
    ToggleSwitchModule
  ],
  templateUrl: './scheduler-shell.component.html',
  styleUrls: ['./scheduler-shell.component.scss']
})
export class SchedulerShellComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  readonly auth = inject(AuthService);

  readonly rooms = signal<Room[]>([]);
  readonly bookings = signal<BookingInstance[]>([]);
  readonly stats = signal<DashboardStats>({ totalRooms: 0, bookingsToday: 0, adminConsentGranted: false, graphSyncActive: false });
  readonly profile = signal<Profile>(DEFAULT_PROFILE);
  readonly profileLoaded = signal(false);
  readonly profileLoadFailed = signal(false);
  readonly activeView = signal<'dashboard' | 'rooms' | 'calendar' | 'admin' | 'settings'>('dashboard');
  readonly roomDialogVisible = signal(false);
  readonly bookingDrawerVisible = signal(false);
  readonly editingRoom = signal<Room | null>(null);
  readonly savingMessage = signal('');
  readonly adminOnboardingRoute = environment.production ? '/admin-onboarding' : '/dev-admin-onboarding';
  readonly devAdminRoute = '/dev-admin';
  readonly devUserRoute = '/dev-user';
  readonly isDevelopment = !environment.production;
  readonly devRole = signal<DevelopmentRole>('admin');
  readonly emailSeparator = /[,;\s]+/;
  readonly timeZoneOptions = buildTimeZoneOptions();

  readonly isAdmin = computed(() => this.profile().roles.includes('OrgAdmin'));
  readonly roomOptions = computed(() => this.rooms()
    .filter((room) => room.isActive)
    .map((room) => ({
      label: `${room.name} - ${room.capacity} seats`,
      value: room.id
    })));
  readonly recurrenceOptions = [
    { label: 'Does not repeat', value: 'None' },
    { label: 'Daily', value: 'Daily' },
    { label: 'Weekly', value: 'Weekly' },
    { label: 'Monthly', value: 'Monthly' }
  ];

  readonly calendarOptions = computed<CalendarOptions>(() => ({
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'timeGridWeek',
    height: 560,
    nowIndicator: true,
    selectable: true,
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay'
    },
    events: this.bookings().map((booking) => ({
      id: booking.id,
      title: `${booking.roomName} - ${booking.subject}`,
      start: booking.startAt,
      end: booking.endAt,
      backgroundColor: booking.isRecurring ? '#0f766e' : '#1d4ed8',
      borderColor: booking.isRecurring ? '#0f766e' : '#1d4ed8'
    })),
    select: (selection) => {
      this.bookingForm.patchValue({
        startAt: selection.start,
        endAt: selection.end
      });
      this.bookingDrawerVisible.set(true);
    }
  }));

  readonly roomForm = this.fb.nonNullable.group({
    name: ['', Validators.required],
    floor: ['', Validators.required],
    location: ['', Validators.required],
    capacity: [8, [Validators.required, Validators.min(1)]],
    amenities: [''],
    exchangeEmail: [''],
    isActive: [true]
  });

  readonly bookingForm = this.fb.nonNullable.group({
    subject: ['', Validators.required],
    roomId: ['', Validators.required],
    to: [[] as string[]],
    cc: [[] as string[]],
    bcc: [[] as string[]],
    body: [''],
    startAt: [new Date(), Validators.required],
    endAt: [new Date(Date.now() + 60 * 60 * 1000), Validators.required],
    timeZone: [DEFAULT_TIME_ZONE, Validators.required],
    recurrenceType: ['None'],
    recurrenceInterval: [1, [Validators.required, Validators.min(1)]],
    recurrenceUntil: [null as Date | null]
  });

  ngOnInit(): void {
    this.applyDevelopmentRole();
    this.loadAll();
  }

  switchView(view: 'dashboard' | 'rooms' | 'calendar' | 'admin' | 'settings'): void {
    if (view === 'admin' && !this.isAdmin()) {
      this.activeView.set('dashboard');
      return;
    }

    this.activeView.set(view);
  }

  setDevelopmentRole(role: DevelopmentRole): void {
    if (!this.isDevelopment) {
      return;
    }

    localStorage.setItem('devRole', role);
    this.devRole.set(role);
    if (role === 'user' && this.activeView() === 'admin') {
      this.activeView.set('dashboard');
    }

    this.profile.set(DEFAULT_PROFILE);
    this.profileLoaded.set(false);
    this.profileLoadFailed.set(false);
    this.savingMessage.set('');
    this.loadAll();
  }

  loadAll(): void {
    this.api.me().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        this.profileLoaded.set(true);
        this.profileLoadFailed.set(false);
      },
      error: () => {
        this.profile.set(DEFAULT_PROFILE);
        this.profileLoaded.set(false);
        this.profileLoadFailed.set(true);
        this.savingMessage.set('Profile and tenant access could not be loaded. Please sign in again or contact an administrator.');
      }
    });
    this.api.stats().subscribe({ next: (stats) => this.stats.set(stats), error: () => this.savingMessage.set('Dashboard stats could not be loaded.') });
    this.api.rooms().subscribe({ next: (rooms) => this.rooms.set(rooms), error: () => this.savingMessage.set('Rooms could not be loaded. Check API configuration and permissions.') });
    this.loadBookings();
  }

  loadBookings(): void {
    const start = new Date();
    start.setDate(start.getDate() - 14);
    const end = new Date();
    end.setDate(end.getDate() + 45);

    this.api.bookings(start, end).subscribe({ next: (bookings) => this.bookings.set(bookings), error: () => this.savingMessage.set('Bookings could not be loaded. Check API configuration and permissions.') });
  }

  openNewRoom(): void {
    this.editingRoom.set(null);
    this.roomForm.reset({ name: '', floor: '', location: '', capacity: 8, amenities: '', exchangeEmail: '', isActive: true });
    this.roomDialogVisible.set(true);
  }

  editRoom(room: Room): void {
    this.editingRoom.set(room);
    this.roomForm.setValue({
      name: room.name,
      floor: room.floor,
      location: room.location,
      capacity: room.capacity,
      amenities: room.amenities ?? '',
      exchangeEmail: room.exchangeEmail ?? '',
      isActive: room.isActive
    });
    this.roomDialogVisible.set(true);
  }

  saveRoom(): void {
    if (this.roomForm.invalid) {
      this.roomForm.markAllAsTouched();
      return;
    }

    const request = this.roomForm.getRawValue() as UpsertRoomRequest;
    const current = this.editingRoom();
    const save$ = current ? this.api.updateRoom(current.id, request) : this.api.createRoom(request);

    save$.subscribe({
      next: () => {
        this.roomDialogVisible.set(false);
        this.loadAll();
      },
      error: () => this.savingMessage.set('Room could not be saved. Check API configuration and permissions.')
    });
  }

  deleteRoom(room: Room): void {
    this.api.deleteRoom(room.id).subscribe({ next: () => this.loadAll() });
  }

  submitBooking(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const value = this.bookingForm.getRawValue();
    const recurrenceType = value.recurrenceType as 'None' | 'Daily' | 'Weekly' | 'Monthly';
    this.api.createBooking(buildCreateBookingRequest({
      ...value,
      recurrenceType
    } as BookingFormValue)).subscribe({
      next: () => {
        this.bookingDrawerVisible.set(false);
        this.loadAll();
      },
      error: () => this.savingMessage.set('Meeting invite could not be sent or the room is already reserved for that time.')
    });
  }

  private applyDevelopmentRole(): void {
    if (!this.isDevelopment) {
      return;
    }

    const role = resolveDevelopmentRole(window.location.pathname, localStorage.getItem('devRole'));
    localStorage.setItem('devRole', role);
    this.devRole.set(role);
  }
}
