public class SerializablePlayerState
{
    public string Name { get; set; }
    public string Gender { get; set; }
    public string Archetype { get; set; }
    public int Coins { get; set; }
    public int MaxStamina { get; set; }
    public int Stamina { get; set; }
    public int MaxHealth { get; set; }
    public int Health { get; set; }
    public int Level { get; set; }
    public int CurrentXP { get; set; }
    public List<string> InventoryItems { get; set; } = new List<string>();
    public List<string> SelectedCards { get; set; } = new List<string>();

    // Letter Queue System
    public List<SerializableLetter> LetterQueue { get; set; } = new List<SerializableLetter>();
    public Dictionary<string, int> ConnectionTokens { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, Dictionary<string, int>> NPCTokens { get; set; } = new Dictionary<string, Dictionary<string, int>>();

    // Physical Letter Carrying
    public List<SerializableLetter> CarriedLetters { get; set; } = new List<SerializableLetter>();

    // Queue manipulation tracking
    public int LastMorningSwapDay { get; set; } = -1;
    public int LastLetterBoardDay { get; set; } = -1;
    public List<SerializableLetter> DailyBoardLetters { get; set; } = new List<SerializableLetter>();

    // Letter history tracking
    public Dictionary<string, SerializableLetterHistory> NPCLetterHistory { get; set; } = new Dictionary<string, SerializableLetterHistory>();

    // Standing Obligations System
    public List<SerializableStandingObligation> StandingObligations { get; set; } = new List<SerializableStandingObligation>();

    // Token Favor System
    public List<string> PurchasedFavors { get; set; } = new List<string>();
    public List<string> UnlockedLocationIds { get; set; } = new List<string>();
    public List<string> UnlockedServices { get; set; } = new List<string>();

    // Scenario tracking
    public List<SerializableLetter> DeliveredLetters { get; set; } = new List<SerializableLetter>();
    public int TotalLettersDelivered { get; set; } = 0;
    public int TotalLettersExpired { get; set; } = 0;
    public int TotalTokensSpent { get; set; } = 0;

    // Patron tracking
    public int PatronLeverage { get; set; } = 0;
}

public class SerializableLetter
{
    public string Id { get; set; }
    public string SenderName { get; set; }
    public string RecipientName { get; set; }
    public int Deadline { get; set; }
    public int Payment { get; set; }
    public string TokenType { get; set; }
    public string State { get; set; }
    public int QueuePosition { get; set; }
    public string Size { get; set; }
    public bool IsFromPatron { get; set; }
    public string PhysicalProperties { get; set; }
    public string RequiredEquipment { get; set; }
    public string SenderId { get; set; }
    public string RecipientId { get; set; }
    public int DaysInQueue { get; set; }
    public string Description { get; set; }
    public bool IsGenerated { get; set; }
    public string GenerationReason { get; set; }
    public List<string> UnlocksLetterIds { get; set; }
    public string ParentLetterId { get; set; }
    public bool IsChainLetter { get; set; }
    public string Message { get; set; }
}

public class SerializableLetterHistory
{
    public int DeliveredCount { get; set; }
    public int SkippedCount { get; set; }
    public int ExpiredCount { get; set; }
    public DateTime LastInteraction { get; set; }
}

public class SerializableStandingObligation
{
    public string ID { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Source { get; set; }
    public List<string> BenefitEffects { get; set; }
    public List<string> ConstraintEffects { get; set; }
    public string RelatedTokenType { get; set; }
    public int DateAccepted { get; set; }
    public bool IsActive { get; set; }
    public int DaysSinceLastForcedLetter { get; set; }
}