public class ActionAvailabilityService
{
    public bool IsActionAvailable(ActionTemplate template, LocationProperties locationProperties)
    {
        foreach (LocationPropertyCondition condition in template.AvailabilityConditions)
        {
            if (!condition.IsMet(locationProperties)) // Use the Location object directly
            {
                return false;
            }
        }
        return true; // All conditions met
    }
}