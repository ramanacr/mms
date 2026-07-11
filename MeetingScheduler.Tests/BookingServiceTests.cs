using MeetingScheduler.Api.Dtos;
using MeetingScheduler.Api.Infrastructure;
using MeetingScheduler.Api.Models;

namespace MeetingScheduler.Tests;

public sealed class BookingServiceTests
{
    [Fact]
    public async Task Rejects_partial_overlap_for_same_room()
    {
        var (db, service, _, _) = TestDb.CreateBookingHarness();
        var room = await AddRoomAsync(db);

        await service.CreateBookingAsync(new CreateBookingRequest(room.Id, "Planning", [], new DateTime(2026, 5, 1, 9, 0, 0), new DateTime(2026, 5, 1, 10, 0, 0), "UTC", null));

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CreateBookingAsync(new CreateBookingRequest(room.Id, "Overlap", [], new DateTime(2026, 5, 1, 9, 30, 0), new DateTime(2026, 5, 1, 10, 30, 0), "UTC", null)));
    }

    [Fact]
    public async Task Allows_adjacent_booking_for_same_room()
    {
        var (db, service, _, _) = TestDb.CreateBookingHarness();
        var room = await AddRoomAsync(db);

        await service.CreateBookingAsync(new CreateBookingRequest(room.Id, "Planning", [], new DateTime(2026, 5, 1, 9, 0, 0), new DateTime(2026, 5, 1, 10, 0, 0), "UTC", null));
        var result = await service.CreateBookingAsync(new CreateBookingRequest(room.Id, "Next", [], new DateTime(2026, 5, 1, 10, 0, 0), new DateTime(2026, 5, 1, 11, 0, 0), "UTC", null));

        Assert.Single(result.Instances);
    }

    [Fact]
    public async Task Allows_overlap_for_different_room()
    {
        var (db, service, _, _) = TestDb.CreateBookingHarness();
        var roomA = await AddRoomAsync(db, "A");
        var roomB = await AddRoomAsync(db, "B");

        await service.CreateBookingAsync(new CreateBookingRequest(roomA.Id, "Planning", [], new DateTime(2026, 5, 1, 9, 0, 0), new DateTime(2026, 5, 1, 10, 0, 0), "UTC", null));
        var result = await service.CreateBookingAsync(new CreateBookingRequest(roomB.Id, "Parallel", [], new DateTime(2026, 5, 1, 9, 30, 0), new DateTime(2026, 5, 1, 10, 30, 0), "UTC", null));

        Assert.Single(result.Instances);
    }

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
}
