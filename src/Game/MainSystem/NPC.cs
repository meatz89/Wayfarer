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
    Always,           // Available all time periods (innkeepers, guards)
    Market_Hours,     // Morning + Afternoon (traders, merchants)
    Workshop_Hours,   // Dawn + Morning + Afternoon (craftsmen)
    Library_Hours,    // Morning + Afternoon (scholars, librarians)
    Business_Hours,   // Morning + Afternoon (caravan masters, officials)
    Morning_Evening,  // Morning + Evening (village elders)
    Morning_Afternoon, // Morning + Afternoon (route planners, miners)
    Afternoon_Evening, // Afternoon + Evening (exotic traders)
    Evening_Only,     // Evening only (tavern keepers, entertainers)
    Morning_Only,     // Morning only (harbor masters)
    Afternoon_Only,   // Afternoon only (diplomatic couriers)
    Evening_Night,    // Evening + Night (information brokers, sea captains)
    Dawn_Only,        // Dawn only (early departing transport, farmers)
    Night_Only        // Night only (guards, special services)
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
    public string ProfessionDescription
    {
        get
        {
            return Profession.ToString().Replace('_', ' ');
        }
    }

    public string SocialClassDescription
    {
        get
        {
            return SocialClass.ToString().Replace('_', ' ');
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
