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
    FreeInitiative,      // Next SPEAK 0 initiative

    // Manipulations
    ResetCadence,    // Cadence = 0
    RefreshInitiative    // Initiative = max
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
        CreatedAt = DateTime.Now;
    }

    // Constructor with observation defaults
    public ObservationCard(string id, string description) : this()
    {
        Id = id;
        Title = description;
        InitiativeCost = 1;
        Difficulty = Difficulty.VeryEasy; // 85%
        CardType = CardType.Observation; // Mark as observation
        Persistence = PersistenceType.Statement; // Observations persist through LISTEN
        SuccessType = SuccessEffectType.None; // Default, can be overridden
        PersonalityTypes = new List<string>();
        LevelBonuses = new List<CardLevelBonus>();
        VerbPhrase = "";
        DialogueText = "";
    }

    public bool IsExpired()
    {
        return ExpirationTime.HasValue && DateTime.Now > ExpirationTime.Value;
    }

    public void SetExpiration(int segments)
    {
        ExpirationSegments = segments;
        ExpirationTime = DateTime.Now.AddMinutes(segments * 10); // 10 minutes per segment
    }

    public void UpdateDecayState(DateTime currentGameTime)
    {
        // Simple decay based on creation time
        TimeSpan age = currentGameTime - CreatedAt;
        if (age.TotalMinutes > 1440) // 144 segments expiration
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
        int ageInSegments = (int)(age.TotalMinutes / 10);
        if (ageInSegments < 6) return "Fresh"; // Fresh state (< 6 segments)
        if (ageInSegments < 36) return "Recent"; // Recent state (< 36 segments)
        if (ageInSegments < 144) return "Aging"; // Aging state (< 144 segments)
        return "Stale";
    }

    public string GetDecayStateDescription(DateTime currentTime)
    {
        TimeSpan age = currentTime - CreatedAt;
        int ageInSegments = (int)(age.TotalMinutes / 10);
        if (ageInSegments < 6) return "Fresh"; // Fresh state (< 6 segments)
        if (ageInSegments < 36) return "Recent"; // Recent state (< 36 segments)
        if (ageInSegments < 144) return "Aging"; // Aging state (< 144 segments)
        return "Stale";
    }
}