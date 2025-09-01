public class SuccessTerms
{
    public int DeadlineHours { get; set; }
    public int QueuePosition { get; set; } // 0 = flexible
    public int Payment { get; set; }
    
    // Additional success conditions
    public string DestinationLocation { get; set; } // For delivery goals
    public string RequiredNpc { get; set; } // For meeting goals
    public string BurdenToRemove { get; set; } // For resolution goals
}