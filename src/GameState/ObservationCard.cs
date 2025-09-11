using System;
using System.Collections.Generic;

public enum ObservationEffectType
{
    // Atmosphere setters
    SetInformed,     // Next card cannot fail
    SetExposed,      // Double flow changes
    SetSynchronized, // Next effect twice
    SetPressured,    // -1 card on LISTEN

    // Cost bypasses
    FreePatience,    // Next action 0 patience
    FreeFocus,      // Next SPEAK 0 focus

    // Manipulations
    ResetFlow,    // Flow = 0
    RefreshFocus    // Focus = max
}

public class ObservationCard : ConversationCard
{
    public ObservationEffectType UniqueEffect { get; set; }
    public int ExpirationSegments { get; set; }
    public DateTime? ExpirationTime { get; set; }

    // Source information
    public string ObservationId { get; set; }
    public string ItemName { get; set; }
    public string LocationDiscovered { get; set; }
    public string TimeDiscovered { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsPlayable { get; set; } = true;

    // Constructor enforces observation rules
    public ObservationCard()
    {
        Focus = 1;
        Difficulty = Difficulty.VeryEasy; // 85%
        CardType = CardType.Observation; // Mark as observation
        Properties.Add(CardProperty.Persistent); // Always persistent
        CreatedAt = DateTime.Now;
    }

    public bool IsExpired()
    {
        return ExpirationTime.HasValue && DateTime.Now > ExpirationTime.Value;
    }

    public void SetExpiration(int segments)
    {
        ExpirationSegments = segments;
        ExpirationTime = DateTime.Now.AddMinutes(segments * 30); // 30 minutes per segment
    }

    public void UpdateDecayState(DateTime currentGameTime)
    {
        // Simple decay based on creation time
        TimeSpan age = currentGameTime - CreatedAt;
        if (age.TotalMinutes > 1440) // 24 hours * 60 minutes
        {
            IsPlayable = false;
        }
    }

    public void UpdateDecayState()
    {
        // Overload with current time
        UpdateDecayState(DateTime.Now);
    }

    public string GetDecayStateDescription()
    {
        if (IsExpired()) return "Expired";

        TimeSpan age = DateTime.Now - CreatedAt;
        if (age.TotalMinutes < 60) return "Fresh"; // 1 hour
        if (age.TotalMinutes < 360) return "Recent"; // 6 hours
        if (age.TotalMinutes < 1440) return "Aging"; // 24 hours
        return "Stale";
    }

    public string GetDecayStateDescription(DateTime currentTime)
    {
        TimeSpan age = currentTime - CreatedAt;
        if (age.TotalMinutes < 60) return "Fresh"; // 1 hour
        if (age.TotalMinutes < 360) return "Recent"; // 6 hours
        if (age.TotalMinutes < 1440) return "Aging"; // 24 hours
        return "Stale";
    }

    // Create from a base conversation card with observation properties
    public static ObservationCard FromConversationCard(ConversationCard card)
    {
        ObservationCard observation = new ObservationCard
        {
            Id = card.Id,
            Description = card.Description,
            ObservationId = card.Id,
            ItemName = "Unknown",
            LocationDiscovered = "Unknown",
            TimeDiscovered = DateTime.Now.ToString(),
            DialogueFragment = card.DialogueFragment,
            VerbPhrase = card.VerbPhrase,
            Focus = card.Focus,
            Difficulty = card.Difficulty,
            TokenType = card.TokenType,
            SuccessEffect = card.SuccessEffect,
            FailureEffect = card.FailureEffect,
            ExhaustEffect = card.ExhaustEffect,
            CreatedAt = DateTime.Now
        };

        return observation;
    }

    public static ObservationCard FromConversationCard(ConversationCard card, string source, string location)
    {
        ObservationCard observation = new ObservationCard
        {
            Id = card.Id,
            Description = card.Description,
            DialogueFragment = card.DialogueFragment,
            VerbPhrase = card.VerbPhrase,
            TokenType = card.TokenType,
            Focus = card.Focus,
            Difficulty = card.Difficulty,
            SuccessEffect = card.SuccessEffect,
            FailureEffect = card.FailureEffect,
            ExhaustEffect = card.ExhaustEffect,
            ObservationId = source,
            LocationDiscovered = location,
            CreatedAt = DateTime.Now
        };

        // Copy properties and set card type
        observation.Properties = new List<CardProperty>(card.Properties);
        observation.CardType = CardType.Observation;

        return observation;
    }

    public ConversationCard ConversationCard { get; set; }
}