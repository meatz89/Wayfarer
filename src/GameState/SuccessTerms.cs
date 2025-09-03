public class SuccessTerms
{
    public int DeadlineHours { get; set; }
    public int QueuePosition { get; set; } // 0 = flexible
    public int Payment { get; set; }

    // Additional success conditions
    public string DestinationLocation { get; set; } // For delivery requests
    public string RequiredNpc { get; set; } // For meeting requests
    public string BurdenToRemove { get; set; } // For resolution requests
}