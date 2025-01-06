public class ActionAvailabilityService
{
    public bool IsActionAvailable(ActionTemplate template, Location location)
    {
        foreach (LocationPropertyCondition condition in template.AvailabilityConditions)
        {
            if (!condition.IsMet(location)) // Use the Location object directly
            {
                return false;
            }
        }
        return true; // All conditions met
    }
}