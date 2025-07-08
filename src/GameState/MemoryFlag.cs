public class MemoryFlag
{
    public string Key { get; set; }
    public string Description { get; set; }
    public object CreationDay { get; set; }
    public object ExpirationDay { get; set; }
    public int Importance { get; set; }

    public bool IsActive(int currentDay)
    {
        return true;
    }
}