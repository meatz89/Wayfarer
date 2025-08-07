
public class NPC
{
    // Identity
    public string ID { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }
    public string SpotId { get; set; }

    // Categorical Properties for Logical System Interactions
    public Professions Profession { get; set; }

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // NPCs are always available - no schedule system
    public List<ServiceTypes> ProvidedServices { get; set; } = new List<ServiceTypes>();
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;

    // Letter Queue Properties
    public List<ConnectionType> LetterTokenTypes { get; set; } = new List<ConnectionType>();

    // Work Properties
    public bool OffersWork => ProvidedServices.Contains(ServiceTypes.Work);

    // Helper methods for UI display
    public string ProfessionDescription => Profession.ToString().Replace('_', ' ');

    public string ScheduleDescription => "Always available";

    public string ProvidedServicesDescription => ProvidedServices.Any()
        ? $"Services: {string.Join(", ", ProvidedServices.Select(s => s.ToString().Replace('_', ' ')))}"
        : "No services available";

    public bool IsAvailable(TimeBlocks currentTime)
    {
        // NPCs are always available by default
        return true;
    }

    public bool IsAvailableAtTime(string locationSpotId, TimeBlocks currentTime)
    {
        // NPCs are always available by default
        // Check if NPC is at the specified location spot
        return SpotId == locationSpotId && IsAvailable(currentTime);
    }

    public bool CanProvideService(ServiceTypes requestedService)
    {
        return ProvidedServices.Contains(requestedService);
    }

    internal bool IsAvailableAtLocation(string? spotID)
    {
        // NPCs are available at their assigned location
        return !string.IsNullOrEmpty(spotID) && Location == spotID;
    }

    // Letter generation methods for VerbContextualizer
    public bool HasLetterToSend()
    {
        // Simple logic: NPCs have letters occasionally
        // In full implementation, would check NPC state, relationships, etc.
        return new Random().Next(3) == 0; // 33% chance
    }

    public Letter GenerateLetter()
    {
        // Generate a simple letter from this NPC
        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            SenderId = this.ID,
            SenderName = this.Name,
            RecipientId = "player_contact_" + new Random().Next(1, 5),
            RecipientName = "Contact " + new Random().Next(1, 5),
            Description = $"Letter from {Name} about {Profession} matters",
            TokenType = LetterTokenTypes.FirstOrDefault(),
            Stakes = StakeType.REPUTATION,
            DeadlineInDays = new Random().Next(2, 7),
            QueuePosition = 6, // Add to back of queue
            State = LetterState.Offered
        };
        return letter;
    }
}
