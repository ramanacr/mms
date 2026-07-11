# Book a Room Meeting Scheduler Design

## Goal

Make MMS behave as a Microsoft meeting scheduler that also books the selected room, sends a meeting invite to recipients, and includes the room name/resource in the invite.

## Current State

MMS already has an Angular scheduling shell, a `Book a Room` drawer, room inventory, booking conflict checks, recurrence expansion, and Microsoft Graph event creation. The current Graph event sets the room as the event location, but it only sends human attendees from the request. A room with `ExchangeEmail` is not yet sent as a room resource attendee.

## Recommended Approach

Use Microsoft Graph event creation as the source of the meeting invite. When a selected room has `ExchangeEmail`, add it to the Graph event attendees with `AttendeeType.Resource`. Keep all user-entered recipients as required attendees. Keep `Location.DisplayName` as the room name and `LocationEmailAddress` as the room mailbox.

This matches Microsoft Graph's event attendee model, where attendees can be people or Exchange resources such as meeting rooms, and the attendee type can be `resource`.

## Alternatives Considered

1. **Recommended: add the room as a Graph resource attendee**
   - Pros: Microsoft-native room booking, lets Exchange room mailbox policy accept or decline, keeps the meeting invite in Outlook/Teams-compatible form.
   - Cons: Requires rooms to have valid Exchange room mailbox email addresses for full behavior.

2. **Only include room name in event location**
   - Pros: Minimal change; already mostly implemented.
   - Cons: Does not actually book a room mailbox or trigger Exchange resource acceptance/decline.

3. **Create separate room calendar events**
   - Pros: Can represent room reservations even without resource mailbox behavior.
   - Cons: More complex, easier to desynchronize, less compatible with Outlook and Teams scheduling behavior.

## Scope

Included:
- Graph event attendees include all recipient emails as required attendees.
- Graph event attendees include the selected room mailbox as a resource attendee when `MeetingRoom.ExchangeEmail` exists.
- Event location continues to include `room.Name` and `room.ExchangeEmail`.
- The booking form and dashboard copy frame the flow as scheduling a meeting and booking a room.
- Tests verify the room resource attendee behavior and mapper/request behavior.

Deferred:
- Microsoft Teams app integration.
- Graph `findMeetingTimes` or live room availability lookup.
- Automatic room discovery from Microsoft 365 room lists.
- Online meeting creation (`isOnlineMeeting`, Teams meeting join URL).
- Invite body editor and optional/required recipient categories.

## Backend Design

`GraphCalendarService.CreateEventAsync` remains the only production Graph event creation path. It will build attendees in two groups:

- Required human attendees from `CreateBookingRequest.Attendees`.
- One resource attendee from `MeetingRoom.ExchangeEmail`, if the value is not blank.

The resource attendee should use:

- `Type = AttendeeType.Resource`
- `EmailAddress.Address = room.ExchangeEmail.Trim()`
- `EmailAddress.Name = room.Name`

Human attendees should continue to use:

- `Type = AttendeeType.Required`
- `EmailAddress.Address = attendee.Trim()`
- `EmailAddress.Name = attendee.Trim()`

The room email should not be duplicated if the user also enters it as a human attendee. The resource attendee should win, because the selected room is semantically a room resource, not a person.

## Frontend Design

The current drawer remains the main scheduling surface. Copy should shift from generic booking language to meeting scheduling language:

- Drawer header: `Book a Room`
- Primary action: `Send Invite & Book Room`
- Calendar/dashboard buttons: `Schedule Meeting`
- Room selector helper text or option label should make the selected room obvious enough for users to understand it will be included in the invite.

No major layout redesign is needed. MMS is an operational scheduler, so the UI should stay compact and task-focused.

## Data Flow

1. User chooses a time range, meeting subject, recipients, recurrence, and room.
2. Angular maps form values to `CreateBookingRequest`.
3. API validates room, subject, and date order.
4. API expands recurrence and checks local room conflicts.
5. API sends a Microsoft Graph event with recipient attendees, room resource attendee, and room location.
6. API stores the booking series and instances with `GraphSeriesId`.
7. UI refreshes rooms, dashboard stats, and calendar bookings.

## Error Handling

Local conflicts continue to return `ConflictException`. Graph failures continue to log the configured scopes and bubble up to the caller. The UI can keep the current concise failure message for this iteration, but the plan should improve the message to mention invite delivery as well as room reservation.

If a room lacks `ExchangeEmail`, MMS still creates the event with the room name as location and stores the booking locally. This supports development and manually managed rooms, but the room will not be Microsoft-resource-booked.

## Testing Strategy

Backend:
- Unit-test attendee construction via a small helper or inspectable collaborator so the Graph SDK call does not need a live Microsoft tenant.
- Verify the room mailbox becomes a resource attendee.
- Verify blank room email produces only human attendees.
- Verify duplicate room email entered in recipients is not duplicated.

Frontend:
- Unit-test `buildCreateBookingRequest` for trimmed recipient parsing and recurrence mapping.
- Unit-test scheduler copy constants or component text if exposed in a testable way.

Manual:
- In development mode, create a room with `ExchangeEmail`.
- Schedule a meeting with at least one attendee.
- Confirm the API accepts the booking and the calendar refreshes.

## Open Decisions

The first implementation should not create Teams online meetings. That belongs in a later Teams integration milestone after the room resource invite behavior is working.

