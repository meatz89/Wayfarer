using System;

public enum ObservationEffectType
{
    // Atmosphere setters
    SetInformed,     // Next card cannot fail
    SetExposed,      // Double comfort changes
    SetSynchronized, // Next effect twice
    SetPressured,    // -1 card on LISTEN

    // Cost bypasses
    FreePatience,    // Next action 0 patience
    FreeWeight,      // Next SPEAK 0 weight

    // Manipulations
    ResetComfort,    // Comfort = 0
    RefreshWeight    // Weight = max
}

public class ObservationCard : ConversationCard
{
    public ObservationEffectType UniqueEffect { get; set; }
    public int ExpirationHours { get; set; }
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
        Weight = 1;
        Difficulty = Difficulty.VeryEasy; // 85%
        Properties.Add(CardProperty.Persistent); // Always persistent
        Properties.Add(CardProperty.Observable); // Mark as observation
        EffectType = CardEffectType.ObservationEffect;
        CreatedAt = DateTime.Now;
        Type = CardType.Observation;
        Persistence = PersistenceType.Persistent;
        IsSingleUse = true;
    }

    public bool IsExpired()
    {
        return ExpirationTime.HasValue && DateTime.Now > ExpirationTime.Value;
    }

    public void SetExpiration(int hours)
    {
        ExpirationHours = hours;
        ExpirationTime = DateTime.Now.AddHours(hours);
    }

    public void UpdateDecayState(DateTime currentGameTime)
    {
        // Simple decay based on creation time
        TimeSpan age = currentGameTime - CreatedAt;
        if (age.TotalHours > 24)
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
        if (age.TotalHours < 1) return "Fresh";
        if (age.TotalHours < 6) return "Recent";
        if (age.TotalHours < 24) return "Aging";
        return "Stale";
    }

    public string GetDecayStateDescription(DateTime currentTime)
    {
        TimeSpan age = currentTime - CreatedAt;
        if (age.TotalHours < 1) return "Fresh";
        if (age.TotalHours < 6) return "Recent";
        if (age.TotalHours < 24) return "Aging";
        return "Stale";
    }

    // Create from a base conversation card with observation properties
    public static ObservationCard FromConversationCard(ConversationCard card)
    {
        ObservationCard observation = new ObservationCard
        {
            Id = card.Id,
            Name = card.Name,
            ObservationId = card.ObservationSource ?? card.Id,
            ItemName = card.SourceItem,
            LocationDiscovered = card.Context?.ObservationLocation,
            TimeDiscovered = DateTime.Now.ToString(),
            DialogueFragment = card.DialogueFragment,
            Weight = card.Weight,
            BaseSuccessChance = card.BaseSuccessChance,
            BaseComfortReward = card.BaseComfortReward,
            CreatedAt = DateTime.Now,
            ConversationCard = card
        };

        return observation;
    }

    public static ObservationCard FromConversationCard(ConversationCard card, string source, string location)
    {
        ObservationCard observation = new ObservationCard
        {
            Id = card.Id,
            Name = card.Name,
            Description = card.Description,
            DialogueFragment = card.DialogueFragment,
            VerbPhrase = card.VerbPhrase,
            TokenType = card.TokenType,
            ObservationSource = source,
            LocationDiscovered = location,
            CreatedAt = DateTime.Now
        };

        // Set unique effect based on card properties if available
        if (card.AtmosphereChange.HasValue)
        {
            observation.AtmosphereChange = card.AtmosphereChange;
        }

        return observation;
    }

    // Legacy compatibility
    public ConversationCard ConversationCard { get; set; }
}