namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Data class for specifying information effects in action definitions
/// </summary>
public class InformationEffectData
{
    public string InformationId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public InformationType Type { get; set; }
    public InformationQuality Quality { get; set; } = InformationQuality.Reliable;
    public string Source { get; set; } = "";
    public int Value { get; set; } = 10;
    public bool UpgradeExisting { get; set; } = false;
    public string LocationId { get; set; } = "";
    public string NPCId { get; set; } = "";
    public List<string> RelatedItemIds { get; set; } = new();
    public List<string> RelatedLocationIds { get; set; } = new();

    public InformationEffectData() { }

    public InformationEffectData(string id, string title, InformationType type, string content = "",
                                InformationQuality quality = InformationQuality.Reliable, string source = "")
    {
        InformationId = id;
        Title = title;
        Type = type;
        Content = content;
        Quality = quality;
        Source = source;
    }
}
