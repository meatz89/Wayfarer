using System.Collections.Generic;

/// <summary>
/// DTO for path card collections used in FixedPath segments and event collections
/// </summary>
public class PathCardCollectionDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string NarrativeText { get; set; }
    
    // For FixedPath segments: Reference existing path cards by ID
    public List<string> PathCardIds { get; set; } = new List<string>();
    
    // For event collections: List of event IDs to randomly select from  
    public List<string> EventIds { get; set; } = new List<string>();
    
    // For when this collection is used as an event itself: event cards for this specific event
    public List<string> EventCardIds { get; set; } = new List<string>();
}