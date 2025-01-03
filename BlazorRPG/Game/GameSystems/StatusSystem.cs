public class StatusSystem
{
    public enum StatusTypes { Hungry, Tired, Injured, Homeless }
    private StatusTypes[] activeStatuses;  // Fixed-size array of current statuses

    public void ApplyStatus(StatusTypes status)
    {

    }
    public void RemoveStatus(StatusTypes status)
    {

    }
    public bool HasStatus(StatusTypes status)
    {
        return true;
    }
}
