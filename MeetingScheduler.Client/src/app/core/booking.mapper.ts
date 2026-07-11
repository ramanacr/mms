import { CreateBookingRequest } from './models';

export interface BookingFormValue {
  roomId: string;
  subject: string;
  to: string[];
  cc: string[];
  bcc: string[];
  body: string;
  startAt: string | Date;
  endAt: string | Date;
  timeZone: string;
  recurrenceType: 'None' | 'Daily' | 'Weekly' | 'Monthly';
  recurrenceInterval: number;
  recurrenceUntil: string | Date | null;
}

export function buildCreateBookingRequest(value: BookingFormValue): CreateBookingRequest {
  return {
    roomId: value.roomId,
    subject: value.subject,
    attendees: normalizeRecipientTokens(value.to),
    optionalAttendees: normalizeRecipientTokens(value.cc),
    body: value.body?.trim() || null,
    startAt: toIsoString(value.startAt),
    endAt: toIsoString(value.endAt),
    timeZone: value.timeZone,
    recurrence: value.recurrenceType === 'None' ? null : {
      type: value.recurrenceType,
      interval: value.recurrenceInterval,
      until: value.recurrenceUntil ? toIsoString(value.recurrenceUntil) : null
    }
  };
}

export function normalizeRecipientTokens(tokens: string[]): string[] {
  return tokens
    .flatMap((token) => token.split(/[,;\s]+/))
    .map((email) => email.trim())
    .filter(Boolean)
    .filter((email, index, values) => values.findIndex((value) => value.toLowerCase() === email.toLowerCase()) === index);
}

function toIsoString(value: string | Date): string {
  return value instanceof Date ? value.toISOString() : new Date(value).toISOString();
}
