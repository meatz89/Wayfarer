
// Observation card
public class ObservationCard : ConversationCard
{
    public string ObservationId { get; init; }
    public string ItemName { get; init; }
    public string LocationDiscovered { get; init; }
    public string TimeDiscovered { get; init; }

    // Additional properties needed by PlayerObservationDeck
    public DateTime CreatedAt { get; set; }
    public bool IsPlayable { get; set; } = true;
    public ConversationCard ConversationCard { get; set; }

    public ObservationCard()
    {
        Type = CardType.Observation;
        IsObservation = true;
        Persistence = PersistenceType.Fleeting;
        IsSingleUse = true;
    }

    public static ObservationCard FromConversationCard(ConversationCard card)
    {
        return new ObservationCard
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
}
