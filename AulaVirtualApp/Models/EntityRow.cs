using System.Text.Json.Nodes;

namespace AulaVirtualApp.Models;

public class EntityRow
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public bool HasSubtitle => !string.IsNullOrWhiteSpace(Subtitle);
    public JsonObject Data { get; set; } = new JsonObject();
    public string Id => Data.TryGetPropertyValue("id", out var v) && v != null ? v.ToString() : string.Empty;
}
