/// <summary>
/// DTO for path card collections used in FixedPath segments and event collections
/// </summary>
public class PathCardCollectionDTO
{
public string Id { get; set; }
public string Name { get; set; }
public string NarrativeText { get; set; }

// For FixedPath segments: Actual path cards (not just IDs)
public List<PathCardDTO> PathCards { get; set; } = new List<PathCardDTO>();

// For event collections: Child event collections to randomly select from
public List<PathCardCollectionDTO> Events { get; set; } = new List<PathCardCollectionDTO>();

// For when this collection is used as an event itself: event cards
public List<PathCardDTO> EventCards { get; set; } = new List<PathCardDTO>();

// Temporary parsing properties - used only during JSON loading, not at runtime
public List<string> PathCardIds { get; set; }
public List<string> EventIds { get; set; }
}