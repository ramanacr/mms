import { CreateBookingRequest } from './models';

export interface BookingFormValue {
  roomId: string;
  subject: string;
  attendees: string;
  startAt: string;
  endAt: string;
  recurrenceType: 'None' | 'Daily' | 'Weekly' | 'Monthly';
  recurrenceInterval: number;
  recurrenceUntil: string;
}

export function buildCreateBookingRequest(value: BookingFormValue, timeZone: string): CreateBookingRequest {
  return {
    roomId: value.roomId,
    subject: value.subject,
    attendees: value.attendees.split(',').map((email) => email.trim()).filter(Boolean),
    startAt: new Date(value.startAt).toISOString(),
    endAt: new Date(value.endAt).toISOString(),
    timeZone,
    recurrence: value.recurrenceType === 'None' ? null : {
      type: value.recurrenceType,
      interval: value.recurrenceInterval,
      until: value.recurrenceUntil ? new Date(value.recurrenceUntil).toISOString() : null
    }
  };
}
