public class Notification
{
    public string Event { get; set; } = "";

    public string ResourceType { get; set; } = "";

    public string ResourceName { get; set; } = "";

    public string ResourcePath { get; set; } = "";

    public string ContentType { get; set; } = "";

    public string Content { get; set; } = "";

    public DateTime Timestamp { get; set; }

    public string ValveState { get; set; } = "";
}
