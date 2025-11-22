/// <summary>
/// Generates atmospheric descriptions from categorical location properties.
///
/// TECHNICAL DEBT: This class needs complete refactoring to use LocationCapability flags
/// instead of deleted LocationPropertyType enum. Currently STUBBED to allow compilation.
///
/// TODO: Rebuild description generation using:
/// - LocationCapability flags (functional capabilities)
/// - LocationPrivacy/Safety/Activity/Purpose (categorical dimensions)
/// - Time-specific logic REMOVED (capabilities are static now)
/// </summary>
public class LocationDescriptionGenerator
{
    /// <summary>
    /// STUBBED: Generate location description.
    /// Returns generic placeholder until refactored.
    /// </summary>
    public string GenerateDescription(
        LocationCapability capabilities,
        TimeBlocks currentTime,
        int npcsPresent)
    {
        // STUB: Return basic description until proper refactoring
        return "The location awaits.";
    }

    /// <summary>
    /// STUBBED: Suggest observations.
    /// Returns empty string until refactored.
    /// </summary>
    public string SuggestObservations(
        LocationCapability capabilities,
        TimeBlocks currentTime)
    {
        // STUB: Return empty until proper refactoring
        return "";
    }

    /// <summary>
    /// STUBBED: Generate brief description.
    /// Returns generic placeholder until refactored.
    /// </summary>
    public string GenerateBriefDescription(LocationCapability capabilities)
    {
        // STUB: Return basic description until proper refactoring
        return "The location.";
    }
}
