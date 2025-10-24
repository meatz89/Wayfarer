using System;

/// <summary>
/// Catalog translating categorical conversation properties to mechanical values
/// Pattern: Categorical (fiction) → Mechanical (game design)
/// </summary>
public static class ConversationCatalog
{
    /// <summary>
    /// Get focus cost for dialogue response based on complexity
    /// </summary>
    public static int GetFocusCostForComplexity(ResponseComplexity complexity)
    {
        return complexity switch
        {
            ResponseComplexity.Simple => 0,      // "Yes", "No", casual responses
            ResponseComplexity.Thoughtful => 2,  // Considered response requiring attention
            ResponseComplexity.Deep => 5,        // Emotional or complex engagement
            ResponseComplexity.Insightful => 8,  // Requires high Insight stat
            _ => throw new InvalidOperationException($"Unknown complexity: {complexity}")
        };
    }

    /// <summary>
    /// Get time cost for dialogue response based on complexity
    /// </summary>
    public static int GetTimeCostForComplexity(ResponseComplexity complexity)
    {
        return complexity switch
        {
            ResponseComplexity.Simple => 0,      // Quick response
            ResponseComplexity.Thoughtful => 1,  // Normal conversation pace
            ResponseComplexity.Deep => 2,        // Extended dialogue
            ResponseComplexity.Insightful => 1,  // High skill = efficient
            _ => 1
        };
    }

    /// <summary>
    /// Get default focus cost when not specified
    /// </summary>
    public static int GetDefaultFocusCost()
    {
        return 1;
    }

    /// <summary>
    /// Get default time cost when not specified
    /// </summary>
    public static int GetDefaultTimeCost()
    {
        return 1;
    }

    /// <summary>
    /// Get relationship impact magnitude based on response type
    /// </summary>
    public static int GetRelationshipDelta(ResponseType type)
    {
        return type switch
        {
            ResponseType.Supportive => 5,    // Friendly and helpful
            ResponseType.Empathetic => 7,    // Understanding and caring
            ResponseType.Intimate => 10,     // Deep personal connection
            ResponseType.Neutral => 0,       // No relationship change
            ResponseType.Dismissive => -5,   // Cold or uncaring
            ResponseType.Harsh => -10,       // Cruel or insulting
            _ => 0
        };
    }
}

/// <summary>
/// Categorical complexity levels for dialogue responses
/// </summary>
public enum ResponseComplexity
{
    Simple,
    Thoughtful,
    Deep,
    Insightful
}

/// <summary>
/// Categorical types of dialogue responses for relationship impact
/// </summary>
public enum ResponseType
{
    Supportive,
    Empathetic,
    Intimate,
    Neutral,
    Dismissive,
    Harsh
}
