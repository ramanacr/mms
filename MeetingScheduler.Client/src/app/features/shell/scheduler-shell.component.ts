import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { FullCalendarModule } from '@fullcalendar/angular';
import { CalendarOptions } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import interactionPlugin from '@fullcalendar/interaction';
import timeGridPlugin from '@fullcalendar/timegrid';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
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
import { buildCreateBookingRequest } from '../../core/booking.mapper';
import { BookingInstance, DashboardStats, Room, UpsertRoomRequest } from '../../core/models';

@Component({
  standalone: true,
  selector: 'app-scheduler-shell',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
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
  readonly profile = signal({ displayName: 'Scheduler User', email: '', tenantId: '', roles: ['OrgAdmin', 'OrgUser'] });
  readonly activeView = signal<'dashboard' | 'rooms' | 'calendar' | 'admin' | 'settings'>('dashboard');
  readonly roomDialogVisible = signal(false);
  readonly bookingDrawerVisible = signal(false);
  readonly editingRoom = signal<Room | null>(null);
  readonly savingMessage = signal('');

  readonly isAdmin = computed(() => this.profile().roles.includes('OrgAdmin'));
  readonly roomOptions = computed(() => this.rooms().filter((room) => room.isActive).map((room) => ({ label: room.name, value: room.id })));
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
        startAt: this.toInputValue(selection.start),
        endAt: this.toInputValue(selection.end)
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
    attendees: [''],
    startAt: [this.toInputValue(new Date()), Validators.required],
    endAt: [this.toInputValue(new Date(Date.now() + 60 * 60 * 1000)), Validators.required],
    recurrenceType: ['None'],
    recurrenceInterval: [1, [Validators.required, Validators.min(1)]],
    recurrenceUntil: ['']
  });

  ngOnInit(): void {
    this.loadAll();
  }

  loadAll(): void {
    this.api.me().subscribe({ next: (profile) => this.profile.set(profile), error: () => undefined });
    this.api.stats().subscribe({ next: (stats) => this.stats.set(stats), error: () => undefined });
    this.api.rooms().subscribe({ next: (rooms) => this.rooms.set(rooms), error: () => this.seedDemoRooms() });
    this.loadBookings();
  }

  loadBookings(): void {
    const start = new Date();
    start.setDate(start.getDate() - 14);
    const end = new Date();
    end.setDate(end.getDate() + 45);

    this.api.bookings(start, end).subscribe({ next: (bookings) => this.bookings.set(bookings), error: () => this.seedDemoBookings() });
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
    }, Intl.DateTimeFormat().resolvedOptions().timeZone || 'UTC')).subscribe({
      next: () => {
        this.bookingDrawerVisible.set(false);
        this.loadAll();
      },
      error: () => this.savingMessage.set('Booking failed. The room may already be reserved for that time.')
    });
  }

  private seedDemoRooms(): void {
    const fallbackRooms: Room[] = [
      { id: 'demo-a', name: 'Conference Room A', floor: '3', location: 'Floor 3, West Wing', capacity: 12, amenities: 'Whiteboard, TV, Teams panel', isActive: true },
      { id: 'demo-b', name: 'Focus Studio', floor: '4', location: 'Floor 4, East Wing', capacity: 4, amenities: 'Display, phone booth', isActive: true },
      { id: 'demo-c', name: 'Board Room', floor: '7', location: 'Floor 7, Executive', capacity: 18, amenities: 'Projector, conference phone', isActive: true }
    ];
    this.rooms.set(fallbackRooms);
    this.stats.update((stats) => ({ ...stats, totalRooms: fallbackRooms.length }));
  }

  private seedDemoBookings(): void {
    const now = new Date();
    const demo: BookingInstance[] = [
      {
        id: 'booking-demo-1',
        seriesId: null,
        roomId: 'demo-a',
        roomName: 'Conference Room A',
        subject: 'Product Review',
        organizerEmail: 'alex@contoso.com',
        attendees: ['mira@contoso.com'],
        startAt: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 10, 0).toISOString(),
        endAt: new Date(now.getFullYear(), now.getMonth(), now.getDate(), 11, 0).toISOString(),
        isRecurring: false
      },
      {
        id: 'booking-demo-2',
        seriesId: 'series-demo',
        roomId: 'demo-c',
        roomName: 'Board Room',
        subject: 'Weekly Staff Sync',
        organizerEmail: 'nina@contoso.com',
        attendees: ['ops@contoso.com'],
        startAt: new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1, 14, 0).toISOString(),
        endAt: new Date(now.getFullYear(), now.getMonth(), now.getDate() + 1, 15, 0).toISOString(),
        isRecurring: true
      }
    ];
    this.bookings.set(demo);
    this.stats.update((stats) => ({ ...stats, bookingsToday: 1 }));
  }

  private toInputValue(date: Date): string {
    const offset = date.getTimezoneOffset();
    const local = new Date(date.getTime() - offset * 60_000);
    return local.toISOString().slice(0, 16);
  }
}
