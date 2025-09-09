/// <summary>
/// Individual chunk of narrative content for streaming support.
/// Allows progressive display of narrative content as it's being generated.
/// </summary>
public class NarrativeChunk
{
    /// <summary>
    /// The text content of this narrative chunk.
    /// Can be a partial sentence, complete sentence, or larger text block.
    /// </summary>
    public string Text { get; set; }
    
    /// <summary>
    /// Whether this chunk represents the end of the current narrative stream.
    /// True indicates no more chunks will follow for this generation request.
    /// </summary>
    public bool IsComplete { get; set; }
    
    /// <summary>
    /// Whether this chunk contains NPC dialogue versus environmental narrative.
    /// True for NPC speech, false for environmental description or narrative text.
    /// Used for UI formatting and display styling.
    /// </summary>
    public bool IsDialogue { get; set; }
}