using Wayfarer.GameState;

public class NPC
{
    // Identity
    public string ID { get; set; }
    public string Name { get; set; }
    public string Role { get; set; }
    public string Description { get; set; }
    public string Location { get; set; }

    // Categorical Properties for Logical System Interactions
    public Professions Profession { get; set; }
    public Schedule AvailabilitySchedule { get; set; }
    public List<ServiceTypes> ProvidedServices { get; set; } = new List<ServiceTypes>();
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;
    
    // Contract Generation Properties
    public List<string> ContractCategories { get; set; } = new List<string>();
    
    // Letter Queue Properties
    public List<ConnectionType> LetterTokenTypes { get; set; } = new List<ConnectionType>();

    // Helper methods for UI display
    public string ProfessionDescription
    {
        get
        {
            return Profession.ToString().Replace('_', ' ');
        }
    }

    public string ScheduleDescription
    {
        get
        {
            return AvailabilitySchedule.ToString().Replace('_', ' ');
        }
    }

    public string ProvidedServicesDescription
    {
        get
        {
            return ProvidedServices.Any()
        ? $"Services: {string.Join(", ", ProvidedServices.Select(s => s.ToString().Replace('_', ' ')))}"
        : "No services available";
        }
    }

    public bool IsAvailable(TimeBlocks currentTime)
    {
        return AvailabilitySchedule switch
        {
            Schedule.Always => true,
            Schedule.Market_Hours => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Schedule.Workshop_Hours => currentTime == TimeBlocks.Dawn || currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Schedule.Library_Hours => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Schedule.Business_Hours => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Schedule.Morning_Evening => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Evening,
            Schedule.Morning_Afternoon => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            Schedule.Afternoon_Evening => currentTime == TimeBlocks.Afternoon || currentTime == TimeBlocks.Evening,
            Schedule.Evening_Only => currentTime == TimeBlocks.Evening,
            Schedule.Morning_Only => currentTime == TimeBlocks.Morning,
            Schedule.Afternoon_Only => currentTime == TimeBlocks.Afternoon,
            Schedule.Evening_Night => currentTime == TimeBlocks.Evening || currentTime == TimeBlocks.Night,
            Schedule.Dawn_Only => currentTime == TimeBlocks.Dawn,
            Schedule.Night_Only => currentTime == TimeBlocks.Night,
            _ => false
        };
    }

    public bool CanProvideService(ServiceTypes requestedService)
    {
        return ProvidedServices.Contains(requestedService);
    }
}
