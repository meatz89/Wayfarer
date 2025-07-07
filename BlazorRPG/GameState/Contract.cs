public class Contract
{
    public string Description { get; set; }
    public string ItemRequired { get; set; }
    public string DestinationLocation { get; set; }
    public int StartDay { get; set; }
    public int DueDay { get; set; }
    public int Payment { get; set; }
    public string FailurePenalty { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsFailed { get; set; }

    public string Type { get; set; }  // "Quest", "Mystery", "Job" - flexible text
    public string Location { get; set; }
    public string RelatedCharacter { get; set; }

    // Classification
    public string Status { get; set; } = "Available";

}

