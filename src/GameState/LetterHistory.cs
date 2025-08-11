public class LetterHistory
{
    public int DeliveredCount { get; set; }
    public int SkippedCount { get; set; }
    public int ExpiredCount { get; set; }
    public int RefusedCount { get; set; } // Worse than expired - you CHOSE to break your word
    public DateTime LastInteraction { get; set; }

    public LetterHistory()
    {
        DeliveredCount = 0;
        SkippedCount = 0;
        ExpiredCount = 0;
        RefusedCount = 0;
        LastInteraction = DateTime.Now;
    }

    public void RecordDelivery()
    {
        DeliveredCount++;
        LastInteraction = DateTime.Now;
    }

    public void RecordSkip()
    {
        SkippedCount++;
        LastInteraction = DateTime.Now;
    }

    public void RecordExpiry()
    {
        ExpiredCount++;
        LastInteraction = DateTime.Now;
    }

    public void RecordRefusal()
    {
        RefusedCount++;
        LastInteraction = DateTime.Now;
    }

    public bool HasHistory => DeliveredCount > 0 || SkippedCount > 0 || ExpiredCount > 0 || RefusedCount > 0;
}