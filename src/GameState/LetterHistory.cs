namespace Wayfarer.GameState
{
    public class LetterHistory
    {
        public int DeliveredCount { get; set; }
        public int SkippedCount { get; set; }
        public int ExpiredCount { get; set; }
        public DateTime LastInteraction { get; set; }
        
        public LetterHistory()
        {
            DeliveredCount = 0;
            SkippedCount = 0;
            ExpiredCount = 0;
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
    }
}