namespace Wayfarer.Content;

/// <summary>
/// Static utility for replacing template placeholders with concrete entity names
/// Used during Scene finalization to convert generic templates to specific narratives
/// </summary>
public static class PlaceholderReplacer
{
    /// <summary>
    /// Replace all known placeholders in text with concrete values from context
    /// Placeholders: {NPCName}, {LocationName}, {PlayerName}, {CurrentNPC}
    /// </summary>
    public static string ReplaceAll(string text, SceneSpawnContext context, GameWorld gameWorld)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string result = text;

        // Replace NPC placeholders
        if (context.CurrentNPC != null)
        {
            result = result.Replace("{NPCName}", context.CurrentNPC.Name);
            result = result.Replace("{CurrentNPC}", context.CurrentNPC.Name);
        }

        // Replace Location placeholders
        if (context.CurrentLocation != null)
        {
            result = result.Replace("{LocationName}", context.CurrentLocation.Name);
        }

        // Replace Player placeholder
        Player player = gameWorld.GetPlayer();
        if (player != null)
        {
            result = result.Replace("{PlayerName}", player.Name);
        }

        // Replace Route placeholder (if applicable)
        if (context.CurrentRoute != null)
        {
            result = result.Replace("{RouteName}", context.CurrentRoute.Name);
        }

        return result;
    }

    /// <summary>
    /// Check if text contains any unreplaced placeholders
    /// Useful for validation during development
    /// </summary>
    public static bool HasUnreplacedPlaceholders(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.Contains("{") && text.Contains("}");
    }
}
