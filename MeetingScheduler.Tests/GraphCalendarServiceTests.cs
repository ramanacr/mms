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

        var attendees = GraphCalendarService.CreateAttendees(room, [" alice@contoso.com ", "bob@contoso.com"], []);

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

        var attendees = GraphCalendarService.CreateAttendees(room, ["alice@contoso.com"], []);

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

        var attendees = GraphCalendarService.CreateAttendees(room, ["boardroom@contoso.com", "alice@contoso.com"], []);

        Assert.Equal(2, attendees.Count);
        Assert.Single(attendees, a => a.EmailAddress?.Address == "boardroom@contoso.com");
        Assert.Contains(attendees, a => a.Type == AttendeeType.Resource && a.EmailAddress?.Address == "boardroom@contoso.com");
    }

    [Fact]
    public void CreateAttendees_adds_cc_recipients_as_optional_attendees()
    {
        var room = new MeetingRoom
        {
            Name = "Board Room",
            ExchangeEmail = "boardroom@contoso.com"
        };

        var attendees = GraphCalendarService.CreateAttendees(room, ["alice@contoso.com"], [" bob@contoso.com "]);

        Assert.Contains(attendees, a => a.Type == AttendeeType.Required && a.EmailAddress?.Address == "alice@contoso.com");
        Assert.Contains(attendees, a => a.Type == AttendeeType.Optional && a.EmailAddress?.Address == "bob@contoso.com");
        Assert.Contains(attendees, a => a.Type == AttendeeType.Resource && a.EmailAddress?.Address == "boardroom@contoso.com");
    }

    [Fact]
    public void CreateBody_returns_html_body_when_body_is_present()
    {
        var body = GraphCalendarService.CreateBody(" <p><strong>Please review</strong> before joining.</p> ");

        Assert.NotNull(body);
        Assert.Equal(BodyType.Html, body.ContentType);
        Assert.Equal("<p><strong>Please review</strong> before joining.</p>", body.Content);
    }
}
