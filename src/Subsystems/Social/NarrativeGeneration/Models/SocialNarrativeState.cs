/// <summary>
/// UI state management for narrative streaming and display.
/// Tracks the current state of narrative content being shown to the player.
/// </summary>
public class SocialNarrativeState
{
    /// <summary>
    /// Whether narrative content is currently being streamed/generated.
    /// Used to show loading indicators or prevent user actions during generation.
    /// </summary>
    public bool IsStreaming { get; set; }

    /// <summary>
    /// Current narrative text being built or displayed.
    /// Updated progressively during streaming, holds partial content.
    /// </summary>
    public string CurrentNarrative { get; set; } = string.Empty;

    /// <summary>
    /// Current dialogue text being built or displayed.
    /// Updated progressively during streaming, holds partial NPC speech.
    /// </summary>
    public string CurrentDialogue { get; set; } = string.Empty;

    /// <summary>
    /// Completed narrative text from the previous turn or generation.
    /// Stores finalized environmental/descriptive content.
    /// </summary>
    public string CompletedNarrative { get; set; } = string.Empty;

    /// <summary>
    /// Completed dialogue text from the previous turn or generation.
    /// Stores finalized NPC speech content.
    /// </summary>
    public string CompletedDialogue { get; set; } = string.Empty;

    /// <summary>
    /// Resets all state to prepare for new narrative generation.
    /// Clears both current and completed content, stops streaming.
    /// </summary>
    public void Reset()
    {
        IsStreaming = false;
        CurrentNarrative = string.Empty;
        CurrentDialogue = string.Empty;
        CompletedNarrative = string.Empty;
        CompletedDialogue = string.Empty;
    }

    /// <summary>
    /// Appends text to the current narrative content.
    /// Used during streaming to progressively build environmental text.
    /// </summary>
    /// <param name="text">Text chunk to append to current narrative</param>
    public void AppendNarrative(string text)
    {
        CurrentNarrative += text;
    }

    /// <summary>
    /// Appends text to the current dialogue content.
    /// Used during streaming to progressively build NPC speech.
    /// </summary>
    /// <param name="text">Text chunk to append to current dialogue</param>
    public void AppendDialogue(string text)
    {
        CurrentDialogue += text;
    }

    /// <summary>
    /// Marks the current generation as complete and moves content to completed state.
    /// Stops streaming and preserves final content for display.
    /// </summary>
    public void Complete()
    {
        IsStreaming = false;
        CompletedNarrative = CurrentNarrative;
        CompletedDialogue = CurrentDialogue;
        CurrentNarrative = string.Empty;
        CurrentDialogue = string.Empty;
    }
}