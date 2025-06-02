public class Opportunity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public int AppearanceDay { get; private set; }
    public int ExpirationDay { get; private set; }
    public List<TimeOfDay> AvailableTimes { get; private set; }
    public List<ChoiceTemplate> OpportunityTemplates { get; private set; }

    // Identity
    public string Type { get; set; }  // "Quest", "Mystery", "Job" - flexible text
    public string Location { get; set; }
    public string LocationSpot { get; set; }  // Specific spot where this opportunity is found
    public string RelatedCharacter { get; set; }

    // Classification
    public string Status { get; set; } = "Available";

    public bool IsAvailable(int currentDay, TimeOfDay currentTime)
    {
        return currentDay >= AppearanceDay &&
               currentDay <= ExpirationDay &&
               AvailableTimes.Contains(currentTime);
    }
}

