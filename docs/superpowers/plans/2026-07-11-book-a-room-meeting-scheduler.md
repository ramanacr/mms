# Book a Room Meeting Scheduler Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make MMS schedule a Microsoft meeting invite while booking the selected room as a Microsoft room resource.

**Architecture:** Keep the existing booking pipeline: Angular form -> `CreateBookingRequest` -> `BookingService` conflict check -> `GraphCalendarService` event creation -> EF booking persistence. Add a focused Graph attendee builder so the selected room mailbox becomes a `resource` attendee when configured, while keeping human recipients as `required` attendees.

**Tech Stack:** ASP.NET Core `net10.0`, EF Core, Microsoft Graph SDK `5.105.0`, Angular `21.x`, PrimeNG, Vitest, xUnit.

## Global Constraints

- Do not implement Microsoft Teams app integration in this plan.
- Do not add Graph room discovery or `findMeetingTimes` in this plan.
- Use `AttendeeType.Resource` for the selected room mailbox when `MeetingRoom.ExchangeEmail` is configured.
- Preserve local room conflict checks before Graph event creation.
- Preserve recurring booking support.
- Preserve development behavior when Graph is not configured.

---

## File Structure

- Modify `MeetingScheduler.Api/Services/GraphCalendarService.cs`: add room resource attendee behavior and helper methods for attendee construction.
- Modify `MeetingScheduler.Tests/Fakes/FakeGraphCalendarService.cs`: capture create requests so booking service tests can assert request and room propagation.
- Modify `MeetingScheduler.Tests/BookingServiceTests.cs`: add tests for selected room email flowing to the Graph calendar service fake.
- Create `MeetingScheduler.Tests/GraphCalendarServiceTests.cs`: unit-test the attendee-building helper without requiring live Graph.
- Modify `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.html`: update visible copy to meeting-scheduler language.
- Modify `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.ts`: improve room option labels and booking failure copy.
- Modify `MeetingScheduler.Client/src/app/core/booking.mapper.spec.ts`: strengthen request-mapping tests for recipient trimming.
- Modify `README.md`: document that room mailbox booking requires `ExchangeEmail`.

---

### Task 1: Extract Graph Attendee Construction

**Files:**
- Modify: `MeetingScheduler.Api/Services/GraphCalendarService.cs`
- Create: `MeetingScheduler.Tests/GraphCalendarServiceTests.cs`

**Interfaces:**
- Produces: `internal static List<Attendee> CreateAttendees(MeetingRoom room, IEnumerable<string> attendeeEmails)`
- Consumes: `MeetingRoom.ExchangeEmail`, `MeetingRoom.Name`, Microsoft Graph `Attendee`, `AttendeeType`, `EmailAddress`

- [ ] **Step 1: Write the failing test**

Add `MeetingScheduler.Tests/GraphCalendarServiceTests.cs`:

```csharp
using MeetingScheduler.Api.Models;
using MeetingScheduler.Api.Services;
using Microsoft.Graph.Models;

namespace MeetingScheduler.Tests;

public sealed class GraphCalendarServiceTests
{
    [Fact]
    public void CreateAttendees_adds_humans_as_required_and_room_as_resource()
    {
        var room = new MeetingRoom
        {
            Name = "Board Room",
            ExchangeEmail = "boardroom@contoso.com"
        };

        var attendees = GraphCalendarService.CreateAttendees(room, [" alice@contoso.com ", "bob@contoso.com"]);

        Assert.Equal(3, attendees.Count);
        Assert.Contains(attendees, a => a.Type == AttendeeType.Required && a.EmailAddress?.Address == "alice@contoso.com");
        Assert.Contains(attendees, a => a.Type == AttendeeType.Required && a.EmailAddress?.Address == "bob@contoso.com");
        Assert.Contains(attendees, a =>
            a.Type == AttendeeType.Resource &&
            a.EmailAddress?.Address == "boardroom@contoso.com" &&
            a.EmailAddress?.Name == "Board Room");
    }

    [Fact]
    public void CreateAttendees_omits_resource_when_room_has_no_exchange_email()
    {
        var room = new MeetingRoom { Name = "Focus Room", ExchangeEmail = " " };

        var attendees = GraphCalendarService.CreateAttendees(room, ["alice@contoso.com"]);

        Assert.Single(attendees);
        Assert.Equal(AttendeeType.Required, attendees[0].Type);
        Assert.Equal("alice@contoso.com", attendees[0].EmailAddress?.Address);
    }

    [Fact]
    public void CreateAttendees_does_not_duplicate_room_email_from_human_attendees()
    {
        var room = new MeetingRoom
        {
            Name = "Board Room",
            ExchangeEmail = "boardroom@contoso.com"
        };

        var attendees = GraphCalendarService.CreateAttendees(room, ["boardroom@contoso.com", "alice@contoso.com"]);

        Assert.Equal(2, attendees.Count);
        Assert.Single(attendees, a => a.EmailAddress?.Address == "boardroom@contoso.com");
        Assert.Contains(attendees, a => a.Type == AttendeeType.Resource && a.EmailAddress?.Address == "boardroom@contoso.com");
    }
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj --filter GraphCalendarServiceTests
```

Expected: FAIL because `GraphCalendarService.CreateAttendees` does not exist.

- [ ] **Step 3: Add the attendee helper**

In `MeetingScheduler.Api/Services/GraphCalendarService.cs`, add this method inside `GraphCalendarService`:

```csharp
internal static List<Attendee> CreateAttendees(MeetingRoom room, IEnumerable<string> attendeeEmails)
{
    var roomEmail = string.IsNullOrWhiteSpace(room.ExchangeEmail)
        ? null
        : room.ExchangeEmail.Trim();

    var attendees = attendeeEmails
        .Where(a => !string.IsNullOrWhiteSpace(a))
        .Select(a => a.Trim())
        .Where(a => roomEmail is null || !string.Equals(a, roomEmail, StringComparison.OrdinalIgnoreCase))
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .Select(a => new Attendee
        {
            Type = AttendeeType.Required,
            EmailAddress = new EmailAddress { Address = a, Name = a }
        })
        .ToList();

    if (roomEmail is not null)
    {
        attendees.Add(new Attendee
        {
            Type = AttendeeType.Resource,
            EmailAddress = new EmailAddress { Address = roomEmail, Name = room.Name }
        });
    }

    return attendees;
}
```

- [ ] **Step 4: Run test to verify it passes**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj --filter GraphCalendarServiceTests
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add .\MeetingScheduler.Api\Services\GraphCalendarService.cs .\MeetingScheduler.Tests\GraphCalendarServiceTests.cs
git commit -m "test: cover graph meeting attendees"
```

---

### Task 2: Use Resource Attendees in Graph Event Creation

**Files:**
- Modify: `MeetingScheduler.Api/Services/GraphCalendarService.cs`

**Interfaces:**
- Consumes: `GraphCalendarService.CreateAttendees(MeetingRoom room, IEnumerable<string> attendeeEmails)`
- Produces: Graph `Event.Attendees` containing required humans plus room resource

- [ ] **Step 1: Confirm the helper test covers the required behavior**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj --filter GraphCalendarServiceTests
```

Expected: PASS with coverage for required human attendees, room resource attendees, missing room email, and duplicate room email handling. No live Graph test is added because `GraphServiceClient.Me.Events.PostAsync` is an SDK call and `CreateAttendees` is the stable unit boundary for the behavior in this task.

- [ ] **Step 2: Replace inline attendee construction**

In `GraphCalendarService.CreateEventAsync`, replace:

```csharp
Attendees = request.Attendees
    .Where(a => !string.IsNullOrWhiteSpace(a))
    .Select(a => new Attendee
    {
        Type = AttendeeType.Required,
        EmailAddress = new EmailAddress { Address = a.Trim(), Name = a.Trim() }
    })
    .ToList()
```

with:

```csharp
Attendees = CreateAttendees(room, request.Attendees)
```

- [ ] **Step 3: Run backend tests**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj
```

Expected: PASS.

- [ ] **Step 4: Commit**

```powershell
git add .\MeetingScheduler.Api\Services\GraphCalendarService.cs
git commit -m "feat: book selected room as graph resource"
```

---

### Task 3: Capture Graph Create Requests in Booking Service Tests

**Files:**
- Modify: `MeetingScheduler.Tests/Fakes/FakeGraphCalendarService.cs`
- Modify: `MeetingScheduler.Tests/TestDb.cs`
- Modify: `MeetingScheduler.Tests/BookingServiceTests.cs`

**Interfaces:**
- Produces: `FakeGraphCalendarService.CreatedEvents`
- Produces: `TestDb.CreateBookingHarness()` returning `(AppDbContext Db, BookingService Service, TestTenantProvider Tenant, FakeGraphCalendarService Graph)`

- [ ] **Step 1: Update the fake**

Change `MeetingScheduler.Tests/Fakes/FakeGraphCalendarService.cs` to:

```csharp
using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Models;
using MeetingScheduler.Api.Services;

namespace MeetingScheduler.Tests.Fakes;

public sealed record CapturedGraphEvent(MeetingRoom Room, CreateBookingRequest Request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> Instances);

public sealed class FakeGraphCalendarService : IGraphCalendarService
{
    public List<CapturedGraphEvent> CreatedEvents { get; } = [];
    public List<string> DeletedIds { get; } = [];

    public Task<string?> CreateEventAsync(MeetingRoom room, CreateBookingRequest request, IReadOnlyList<(DateTime StartAt, DateTime EndAt)> instances, CancellationToken cancellationToken = default)
    {
        CreatedEvents.Add(new CapturedGraphEvent(room, request, instances));
        return Task.FromResult<string?>($"graph-{Guid.NewGuid():N}");
    }

    public Task DeleteEventAsync(string graphEventId, CancellationToken cancellationToken = default)
    {
        DeletedIds.Add(graphEventId);
        return Task.CompletedTask;
    }
}
```

- [ ] **Step 2: Return the fake from the harness**

In `MeetingScheduler.Tests/TestDb.cs`, change `CreateBookingHarness` to:

```csharp
public static (AppDbContext Db, BookingService Service, TestTenantProvider Tenant, FakeGraphCalendarService Graph) CreateBookingHarness()
{
    var tenant = new TestTenantProvider();
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString())
        .Options;
    var db = new AppDbContext(options, tenant);
    var graph = new FakeGraphCalendarService();
    var service = new BookingService(
        new EfRepository<Api.Models.MeetingRoom>(db),
        new EfRepository<Api.Models.BookingSeries>(db),
        new EfRepository<Api.Models.BookingInstance>(db),
        new RecurrenceService(),
        graph,
        tenant);

    return (db, service, tenant, graph);
}
```

- [ ] **Step 3: Fix tuple deconstruction in existing tests**

In `MeetingScheduler.Tests/BookingServiceTests.cs`, change each:

```csharp
var (db, service, _) = TestDb.CreateBookingHarness();
```

to:

```csharp
var (db, service, _, _) = TestDb.CreateBookingHarness();
```

- [ ] **Step 4: Add propagation test**

Add this test to `BookingServiceTests`:

```csharp
[Fact]
public async Task Sends_selected_room_and_attendees_to_graph_calendar_service()
{
    var (db, service, _, graph) = TestDb.CreateBookingHarness();
    var room = await AddRoomAsync(db, "Board Room", "boardroom@contoso.com");

    await service.CreateBookingAsync(new CreateBookingRequest(
        room.Id,
        "Planning",
        ["alice@contoso.com"],
        new DateTime(2026, 5, 1, 9, 0, 0),
        new DateTime(2026, 5, 1, 10, 0, 0),
        "UTC",
        null));

    var created = Assert.Single(graph.CreatedEvents);
    Assert.Equal(room.Id, created.Room.Id);
    Assert.Equal("boardroom@contoso.com", created.Room.ExchangeEmail);
    Assert.Equal(["alice@contoso.com"], created.Request.Attendees);
}
```

Update the helper signature:

```csharp
private static async Task<MeetingRoom> AddRoomAsync(Api.Data.AppDbContext db, string name = "Room", string? exchangeEmail = null)
{
    var room = new MeetingRoom
    {
        Name = name,
        Floor = "3",
        Location = "Floor 3",
        Capacity = 8,
        ExchangeEmail = exchangeEmail,
        IsActive = true
    };
    db.MeetingRooms.Add(room);
    await db.SaveChangesAsync();
    return room;
}
```

- [ ] **Step 5: Run backend tests**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\MeetingScheduler.Tests\Fakes\FakeGraphCalendarService.cs .\MeetingScheduler.Tests\TestDb.cs .\MeetingScheduler.Tests\BookingServiceTests.cs
git commit -m "test: capture graph booking requests"
```

---

### Task 4: Update Scheduler UI Copy

**Files:**
- Modify: `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.html`
- Modify: `MeetingScheduler.Client/src/app/features/shell/scheduler-shell.component.ts`

**Interfaces:**
- Produces: `roomOptions` labels that include room name and capacity
- Produces: UI copy that frames the action as sending a meeting invite and booking a room

- [ ] **Step 1: Update room option labels**

In `scheduler-shell.component.ts`, replace:

```typescript
readonly roomOptions = computed(() => this.rooms().filter((room) => room.isActive).map((room) => ({ label: room.name, value: room.id })));
```

with:

```typescript
readonly roomOptions = computed(() => this.rooms()
  .filter((room) => room.isActive)
  .map((room) => ({
    label: `${room.name} - ${room.capacity} seats`,
    value: room.id
  })));
```

- [ ] **Step 2: Update visible copy**

In `scheduler-shell.component.html`, make these replacements:

```html
<span>Room Scheduler</span>
```

to:

```html
<span>MMS</span>
```

```html
<h1>Meeting Room Scheduler</h1>
```

to:

```html
<h1>Microsoft Meeting Scheduler</h1>
```

```html
<p-button label="Create Booking" icon="pi pi-plus" size="small" (onClick)="bookingDrawerVisible.set(true)" />
```

to:

```html
<p-button label="Schedule Meeting" icon="pi pi-plus" size="small" (onClick)="bookingDrawerVisible.set(true)" />
```

Apply the same `Schedule Meeting` label to the calendar view button. Change quick booking:

```html
<h2>Quick Booking</h2>
```

to:

```html
<h2>Quick Meeting</h2>
```

```html
<p-button label="New Booking" icon="pi pi-calendar-plus" styleClass="w-full" (onClick)="bookingDrawerVisible.set(true)" />
```

to:

```html
<p-button label="Schedule Meeting" icon="pi pi-calendar-plus" styleClass="w-full" (onClick)="bookingDrawerVisible.set(true)" />
```

At the drawer footer, replace:

```html
<p-button label="Create Booking" icon="pi pi-send" type="submit" />
```

with:

```html
<p-button label="Send Invite & Book Room" icon="pi pi-send" type="submit" />
```

- [ ] **Step 3: Improve booking failure copy**

In `scheduler-shell.component.ts`, replace:

```typescript
error: () => this.savingMessage.set('Booking failed. The room may already be reserved for that time.')
```

with:

```typescript
error: () => this.savingMessage.set('Meeting invite could not be sent or the room is already reserved for that time.')
```

- [ ] **Step 4: Run frontend tests**

Run:

```powershell
cd .\MeetingScheduler.Client
npm test
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add .\MeetingScheduler.Client\src\app\features\shell\scheduler-shell.component.html .\MeetingScheduler.Client\src\app\features\shell\scheduler-shell.component.ts
git commit -m "feat: clarify meeting scheduling flow"
```

---

### Task 5: Strengthen Booking Mapper Tests

**Files:**
- Modify: `MeetingScheduler.Client/src/app/core/booking.mapper.spec.ts`

**Interfaces:**
- Consumes: `buildCreateBookingRequest(value, timeZone)`
- Produces: Tests proving attendee parsing remains clean for Graph invite creation

- [ ] **Step 1: Add attendee trimming test**

In `booking.mapper.spec.ts`, add:

```typescript
it('trims comma-separated attendees and removes blank entries', () => {
  const request = buildCreateBookingRequest({
    roomId: 'room-1',
    subject: 'Planning',
    attendees: ' alice@contoso.com, , bob@contoso.com ',
    startAt: '2026-05-01T09:00',
    endAt: '2026-05-01T10:00',
    recurrenceType: 'None',
    recurrenceInterval: 1,
    recurrenceUntil: ''
  }, 'Asia/Calcutta');

  expect(request.attendees).toEqual(['alice@contoso.com', 'bob@contoso.com']);
});
```

- [ ] **Step 2: Run mapper test**

Run:

```powershell
cd .\MeetingScheduler.Client
npm test -- booking.mapper.spec.ts
```

Expected: PASS.

- [ ] **Step 3: Commit**

```powershell
git add .\MeetingScheduler.Client\src\app\core\booking.mapper.spec.ts
git commit -m "test: cover meeting attendee mapping"
```

---

### Task 6: Document Room Mailbox Behavior

**Files:**
- Modify: `README.md`

**Interfaces:**
- Produces: Documentation for admins configuring rooms

- [ ] **Step 1: Add README note**

Under the API overview or Notes section, add:

```markdown
## Room Mailbox Booking

For Microsoft-native room booking, configure each room with an Exchange room mailbox in `ExchangeEmail`.
When a user schedules a meeting, MMS sends the selected room mailbox as a Microsoft Graph `resource` attendee and also sets the event location to the room name.

Rooms without `ExchangeEmail` can still be reserved locally in MMS, but they will not receive or process Microsoft room resource invites.
```

- [ ] **Step 2: Run final verification**

Run:

```powershell
dotnet test .\MeetingScheduler.Tests\MeetingScheduler.Tests.csproj
```

Expected: PASS.

Run:

```powershell
cd .\MeetingScheduler.Client
npm test
```

Expected: PASS.

- [ ] **Step 3: Commit**

```powershell
git add .\README.md
git commit -m "docs: describe room mailbox booking"
```

---

## Final Manual Check

- [ ] Start the API in development mode.
- [ ] Start the Angular client.
- [ ] Create or edit a room with `ExchangeEmail = boardroom@contoso.com`.
- [ ] Open `Book a Room`.
- [ ] Enter a subject, at least one attendee, start/end time, and the room.
- [ ] Submit with `Send Invite & Book Room`.
- [ ] Confirm the booking appears in the calendar.
- [ ] In a real Microsoft tenant, confirm the Outlook event includes the room as a resource attendee and the location displays the room name.

## Self-Review Notes

- Spec coverage: Graph room resource invite, UI copy, local conflict preservation, tests, and docs are all mapped to tasks.
- Placeholder scan: no incomplete placeholder wording appears in task steps.
- Type consistency: `CreateAttendees`, `CapturedGraphEvent`, and `CreateBookingHarness` signatures are consistent across tasks.
