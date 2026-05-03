import { describe, expect, it } from 'vitest';
import { buildCreateBookingRequest } from './booking.mapper';

describe('buildCreateBookingRequest', () => {
  it('maps comma-separated attendees and single booking recurrence', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Planning',
      attendees: 'alice@contoso.com, bob@contoso.com, ',
      startAt: '2026-05-01T09:00',
      endAt: '2026-05-01T10:00',
      recurrenceType: 'None',
      recurrenceInterval: 1,
      recurrenceUntil: ''
    }, 'UTC');

    expect(request.attendees).toEqual(['alice@contoso.com', 'bob@contoso.com']);
    expect(request.recurrence).toBeNull();
  });

  it('maps weekly recurrence settings', () => {
    const request = buildCreateBookingRequest({
      roomId: 'room-1',
      subject: 'Staff Sync',
      attendees: '',
      startAt: '2026-05-04T09:00',
      endAt: '2026-05-04T10:00',
      recurrenceType: 'Weekly',
      recurrenceInterval: 2,
      recurrenceUntil: '2026-06-01T09:00'
    }, 'Asia/Calcutta');

    expect(request.timeZone).toBe('Asia/Calcutta');
    expect(request.recurrence).toMatchObject({ type: 'Weekly', interval: 2 });
    expect(request.recurrence?.until).toContain('2026-06');
  });
});
