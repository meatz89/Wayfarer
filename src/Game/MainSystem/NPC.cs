public enum Social_Class
{
    Commoner,
    Merchant,
    Craftsman,
    Minor_Noble,
    Major_Noble
}

public enum Schedule
{
    Morning,
    Afternoon,
    Evening,
    Always,
    Market_Days
}

public enum NPCRelationship
{
    Helpful,
    Neutral,
    Wary,
    Hostile
}

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
    public Social_Class SocialClass { get; set; }
    public Schedule AvailabilitySchedule { get; set; }
    public List<ServiceTypes> ProvidedServices { get; set; } = new List<ServiceTypes>();
    public NPCRelationship PlayerRelationship { get; set; } = NPCRelationship.Neutral;
    
    // Helper methods for UI display
    public string ProfessionDescription => Profession.ToString().Replace('_', ' ');
    public string SocialClassDescription => SocialClass.ToString().Replace('_', ' ');
    public string ScheduleDescription => AvailabilitySchedule.ToString().Replace('_', ' ');
    
    public string ProvidedServicesDescription => ProvidedServices.Any()
        ? $"Services: {string.Join(", ", ProvidedServices.Select(s => s.ToString().Replace('_', ' ')))}"
        : "No services available";
    
    public bool IsAvailable(TimeBlocks currentTime)
    {
        return AvailabilitySchedule switch
        {
            Schedule.Morning => currentTime == TimeBlocks.Morning,
            Schedule.Afternoon => currentTime == TimeBlocks.Afternoon,
            Schedule.Evening => currentTime == TimeBlocks.Evening,
            Schedule.Always => true,
            Schedule.Market_Days => currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon,
            _ => false
        };
    }
    
    public bool CanProvideService(ServiceTypes requestedService)
    {
        return ProvidedServices.Contains(requestedService);
    }
    
    public bool MeetsLocationRequirements(Social_Expectation locationExpectation)
    {
        return locationExpectation switch
        {
            Social_Expectation.Any => true,
            Social_Expectation.Merchant_Class => SocialClass >= Social_Class.Merchant,
            Social_Expectation.Noble_Class => SocialClass >= Social_Class.Minor_Noble,
            Social_Expectation.Professional => SocialClass >= Social_Class.Craftsman,
            _ => false
        };
    }
}
