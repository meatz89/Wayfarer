using System.Collections.Generic;

/// <summary>
/// Data Transfer Object for deserializing NPC data from JSON.
/// Maps to the structure in npcs.json.
/// </summary>
public class NPCDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Profession { get; set; }
    public string LocationId { get; set; }
    public string SpotId { get; set; }
    public string Description { get; set; }
    public string Personality { get; set; }
    public string PersonalityType { get; set; }
    public List<string> Services { get; set; } = new List<string>();
    public List<string> LetterTokenTypes { get; set; } = new List<string>();
    public string Role { get; set; }
    public string AvailabilitySchedule { get; set; }
    public int Tier { get; set; }
    
    // Mechanical properties to replace hardcoded ID checks
    public bool HasLetterDeck { get; set; }
    public bool HasUrgentMeeting { get; set; }
    public string DefaultLetterRecipient { get; set; }
    public bool CrisisIfNegotiationFailed { get; set; }
    
    // Properties from JSON that weren't being parsed
    public string CurrentState { get; set; }
    public int? LetterDeadline { get; set; }
    public UrgentLetterDTO UrgentLetter { get; set; }
    public bool HasLetter { get; set; }
    public ActiveLetterDTO ActiveLetter { get; set; }
    public int? LeavingAt { get; set; }
    public int? WillBeUnavailableAfter { get; set; }
    public bool IsBlockingPath { get; set; }
    public int? RequiresStatusToPass { get; set; }
}

public class UrgentLetterDTO
{
    public string Id { get; set; }
    public string Sender { get; set; }
    public string Recipient { get; set; }
    public int Weight { get; set; }
    public int DeadlineInMinutes { get; set; }
    public string Stakes { get; set; }
    public string Description { get; set; }
}

public class ActiveLetterDTO
{
    public string Id { get; set; }
    public string Sender { get; set; }
    public string Recipient { get; set; }
    public int Weight { get; set; }
    public int DeadlineInMinutes { get; set; }
    public int Reward { get; set; }
    public string Description { get; set; }
}