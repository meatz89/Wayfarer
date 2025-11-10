/// <summary>
/// DTO for navigation-specific data for Navigation interaction type situations
/// </summary>
public class NavigationPayloadDTO
{
public string DestinationId { get; set; }
public bool AutoTriggerScene { get; set; } = false;
}
