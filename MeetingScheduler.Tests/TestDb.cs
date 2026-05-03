using MeetingScheduler.Api.Data;
using MeetingScheduler.Api.Repositories;
using MeetingScheduler.Api.Services;
using MeetingScheduler.Tests.Fakes;
using Microsoft.EntityFrameworkCore;

namespace MeetingScheduler.Tests;

public static class TestDb
{
    public static (AppDbContext Db, BookingService Service, TestTenantProvider Tenant) CreateBookingHarness()
    {
        var tenant = new TestTenantProvider();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var db = new AppDbContext(options, tenant);
        var service = new BookingService(
            new EfRepository<Api.Models.MeetingRoom>(db),
            new EfRepository<Api.Models.BookingSeries>(db),
            new EfRepository<Api.Models.BookingInstance>(db),
            new RecurrenceService(),
            new FakeGraphCalendarService(),
            tenant);

        return (db, service, tenant);
    }
}
