namespace MeetingScheduler.Api.Options;

public sealed class GraphOptions
{
    public const string SectionName = "Graph";

    public string[] Scopes { get; set; } = ["User.Read", "Calendars.ReadWrite"];
}
