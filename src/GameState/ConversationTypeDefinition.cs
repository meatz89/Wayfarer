using System.Collections.Generic;

/// <summary>
/// Defines a conversation type loaded from JSON
/// Fully extensible without code changes
/// </summary>
public class ConversationTypeDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string DeckId { get; set; }
    public string Category { get; set; }  // Links requests to conversation types
    public List<string> AvailableTimeBlocks { get; set; } = new();
    public int DoubtPerListen { get; set; } = 0;
    public bool MomentumErosion { get; set; } = false;
    public int MaxDoubt { get; set; } = 10;
    public int MaxInitiative { get; set; } = 10;
    public DepthDistribution Distribution { get; set; } = new();
}

/// <summary>
/// Depth distribution for conversation types
/// Defines what percentage of cards should come from each depth range
/// </summary>
public class DepthDistribution
{
    public float Foundation { get; set; } = 0.4f; // Depth 1-2
    public float Standard { get; set; } = 0.3f;   // Depth 3-4
    public float Advanced { get; set; } = 0.2f;   // Depth 5-6
    public float Decisive { get; set; } = 0.1f;   // Depth 7-8
}

/// <summary>
/// A collection of card IDs that form a deck definition
/// Reusable across multiple conversation types
/// </summary>
public class CardDeckDefinition
{
    public string Id { get; set; }
    public List<string> CardIds { get; set; } = new();
}