/// <summary>
/// Calculates the time impact of actions including deadline effects
/// </summary>
public class TimeImpactCalculator
{
    private readonly TimeManager _timeManager;
    private readonly GameWorld _gameWorld;
    private readonly ObligationQueueManager _letterQueueManager;

    public TimeImpactCalculator(
        TimeManager timeManager,
        GameWorld gameWorld,
        ObligationQueueManager letterQueueManager)
    {
        _timeManager = timeManager;
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
    }

    public TimeImpactInfo CalculateTimeImpact(int segments)
    {
        int currentSegment = _timeManager.CurrentSegment;
        int currentDay = _timeManager.GetCurrentDay();
        TimeBlocks currentTimeBlock = _timeManager.GetCurrentTimeBlock();

        // Calculate resulting time
        int resultSegment = currentSegment + segments;
        int daysAdvanced = 0;

        // Handle day advancement (24 segments per day)
        while (resultSegment >= 24)
        {
            resultSegment -= 24;
            daysAdvanced++;
        }

        TimeBlocks resultTimeBlock = GetTimeBlockForSegment(resultSegment);
        bool wouldAdvanceDay = daysAdvanced > 0;

        // Calculate deadline impacts
        List<AffectedLetterInfo> deadlineImpacts = CalculateDeadlineImpacts(segments);

        return new TimeImpactInfo
        {
            Segments = segments,
            CurrentTimeBlock = currentTimeBlock,
            ResultTimeBlock = resultTimeBlock,
            WouldAdvanceDay = wouldAdvanceDay,
            DaysAdvanced = daysAdvanced,
            ActiveSegmentsRemaining = _timeManager.SegmentsRemainingInDay,
            LettersExpiring = deadlineImpacts.Count,
            AffectedLetters = deadlineImpacts
        };
    }

    private List<AffectedLetterInfo> CalculateDeadlineImpacts(int segmentsAdvanced)
    {
        List<AffectedLetterInfo> impacts = new List<AffectedLetterInfo>();
        DeliveryObligation[] queue = _letterQueueManager.GetPlayerQueue();

        foreach (DeliveryObligation? letter in queue.Where(l => l != null))
        {
            int segmentsRemaining = letter.DeadlineInSegments - segmentsAdvanced;
            if (segmentsRemaining <= 0 && letter.DeadlineInSegments > 0)
            {
                // This letter would expire
                impacts.Add(new AffectedLetterInfo
                {
                    LetterId = letter.Id,
                    Route = $"{letter.SenderName} → {letter.RecipientName}",
                    CurrentSegmentsRemaining = letter.DeadlineInSegments,
                    ResultSegmentsRemaining = segmentsRemaining
                });
            }
        }

        return impacts;
    }

    private TimeBlocks GetTimeBlockForSegment(int segment)
    {
        return segment switch
        {
            >= 1 and <= 4 => TimeBlocks.Dawn,          // Segments 1-4 (Dawn)
            >= 5 and <= 8 => TimeBlocks.Morning,       // Segments 5-8 (Morning)
            >= 9 and <= 12 => TimeBlocks.Midday,       // Segments 9-12 (Midday)
            >= 13 and <= 16 => TimeBlocks.Afternoon,   // Segments 13-16 (Afternoon)
            >= 17 and <= 20 => TimeBlocks.Evening,     // Segments 17-20 (Evening)
            >= 21 and <= 24 => TimeBlocks.Night,       // Segments 21-24 (Night)
            _ => TimeBlocks.Dawn                       // Default (wrap to dawn)
        };
    }
}

public class TimeImpactInfo
{
    public int Segments { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public TimeBlocks ResultTimeBlock { get; set; }
    public bool WouldAdvanceDay { get; set; }
    public int DaysAdvanced { get; set; }
    public int ActiveSegmentsRemaining { get; set; }
    public int LettersExpiring { get; set; }
    public List<AffectedLetterInfo> AffectedLetters { get; set; }
}

public class AffectedLetterInfo
{
    public string LetterId { get; set; }
    public string Route { get; set; }
    public int CurrentSegmentsRemaining { get; set; }
    public int ResultSegmentsRemaining { get; set; }
}