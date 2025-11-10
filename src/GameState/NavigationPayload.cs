/// <summary>
/// Navigation-specific data for Navigation interaction type situations
/// Defines where navigation leads and whether to trigger scene automatically
/// </summary>
public class NavigationPayload
{
/// <summary>
/// Destination location ID where navigation leads
/// </summary>
public string DestinationId { get; set; }

/// <summary>
/// Whether to automatically trigger scene at destination
/// true = Enter location scene immediately after travel
/// false = Just move to location, player controls when to interact
/// </summary>
public bool AutoTriggerScene { get; set; } = false;
}
