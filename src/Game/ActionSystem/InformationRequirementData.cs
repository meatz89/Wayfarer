namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Data class for specifying information requirements in action definitions
/// </summary>
public class InformationRequirementData
{
    public InformationType RequiredType { get; set; }
    public InformationQuality MinimumQuality { get; set; } = InformationQuality.Reliable;
    public string SpecificInformationId { get; set; } = "";

    public InformationRequirementData() { }

    public InformationRequirementData(InformationType type, InformationQuality quality = InformationQuality.Reliable, string specificId = "")
    {
        RequiredType = type;
        MinimumQuality = quality;
        SpecificInformationId = specificId;
    }
}
