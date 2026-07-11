import { describe, expect, it } from 'vitest';
import { buildCreateBookingRequest } from './booking.mapper';

describe('buildCreateBookingRequest', () => {
  it('maps comma-separated attendees and single booking recurrence', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Planning',
      to: ['alice@contoso.com', 'bob@contoso.com'],
      cc: [],
      bcc: [],
      body: '',
      startAt: '2026-05-01T09:00',
      endAt: '2026-05-01T10:00',
      timeZone: 'UTC',
      recurrenceType: 'None',
      recurrenceInterval: 1,
      recurrenceUntil: ''
    });

    expect(request.attendees).toEqual(['alice@contoso.com', 'bob@contoso.com']);
    expect(request.recurrence).toBeNull();
  });

  it('maps weekly recurrence settings', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Staff Sync',
      to: [],
      cc: [],
      bcc: [],
      body: '',
      startAt: '2026-05-04T09:00',
      endAt: '2026-05-04T10:00',
      timeZone: 'Asia/Calcutta',
      recurrenceType: 'Weekly',
      recurrenceInterval: 2,
      recurrenceUntil: '2026-06-01T09:00'
    });

    expect(request.timeZone).toBe('Asia/Calcutta');
    expect(request.recurrence).toMatchObject({ type: 'Weekly', interval: 2 });
    expect(request.recurrence?.until).toContain('2026-06');
  });

  it('trims comma-separated attendees and removes blank entries', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Planning',
      to: [' alice@contoso.com, ; bob@contoso.com '],
      cc: [],
      bcc: [],
      body: '',
      startAt: '2026-05-01T09:00',
      endAt: '2026-05-01T10:00',
      timeZone: 'Asia/Calcutta',
      recurrenceType: 'None',
      recurrenceInterval: 1,
      recurrenceUntil: ''
    });

    expect(request.attendees).toEqual(['alice@contoso.com', 'bob@contoso.com']);
  });

  it('maps rich meeting fields for Graph invites', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Roadmap Review',
      to: ['alice@contoso.com'],
      cc: [' bob@contoso.com ', 'carol@contoso.com; dana@contoso.com'],
      bcc: ['hidden@contoso.com'],
      body: 'Please review the roadmap before joining.',
      startAt: new Date('2026-05-01T09:00:00+05:30'),
      endAt: new Date('2026-05-01T10:00:00+05:30'),
      timeZone: 'Asia/Calcutta',
      recurrenceType: 'None',
      recurrenceInterval: 1,
      recurrenceUntil: ''
    });

    expect(request.attendees).toEqual(['alice@contoso.com']);
    expect(request.optionalAttendees).toEqual(['bob@contoso.com', 'carol@contoso.com', 'dana@contoso.com']);
    expect(request.body).toBe('Please review the roadmap before joining.');
    expect(request.timeZone).toBe('Asia/Calcutta');
    expect(request.startAt).toBe('2026-05-01T03:30:00.000Z');
    expect(request.endAt).toBe('2026-05-01T04:30:00.000Z');
    expect('bccAttendees' in request).toBe(false);
  });
});
