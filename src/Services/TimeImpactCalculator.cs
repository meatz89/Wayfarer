namespace Wayfarer.Services;

/// <summary>
/// Calculates the time impact of actions including deadline effects
/// </summary>
public class TimeImpactCalculator
{
    private readonly ITimeManager _timeManager;
    private readonly GameWorld _gameWorld;
    private readonly LetterQueueManager _letterQueueManager;

    public TimeImpactCalculator(
        ITimeManager timeManager,
        GameWorld gameWorld,
        LetterQueueManager letterQueueManager)
    {
        _timeManager = timeManager;
        _gameWorld = gameWorld;
        _letterQueueManager = letterQueueManager;
    }

    public TimeImpactInfo CalculateTimeImpact(int hours)
    {
        var currentHour = _timeManager.GetCurrentTimeHours();
        var currentDay = _timeManager.GetCurrentDay();
        var currentTimeBlock = _timeManager.GetCurrentTimeBlock();
        
        // Calculate resulting time
        var resultHour = currentHour + hours;
        var daysAdvanced = 0;
        
        // Handle day advancement
        while (resultHour >= 24)
        {
            resultHour -= 24;
            daysAdvanced++;
        }
        
        var resultTimeBlock = GetTimeBlockForHour(resultHour);
        var wouldAdvanceDay = daysAdvanced > 0;
        
        // Calculate deadline impacts
        var deadlineImpacts = CalculateDeadlineImpacts(daysAdvanced);
        
        return new TimeImpactInfo
        {
            Hours = hours,
            CurrentTimeBlock = currentTimeBlock,
            ResultTimeBlock = resultTimeBlock,
            WouldAdvanceDay = wouldAdvanceDay,
            DaysAdvanced = daysAdvanced,
            ActiveHoursRemaining = _timeManager.HoursRemaining,
            LettersExpiring = deadlineImpacts.Count,
            AffectedLetters = deadlineImpacts
        };
    }
    
    private List<AffectedLetterInfo> CalculateDeadlineImpacts(int daysAdvanced)
    {
        var impacts = new List<AffectedLetterInfo>();
        var queue = _letterQueueManager.GetPlayerQueue();
        
        foreach (var letter in queue.Where(l => l != null))
        {
            var daysRemaining = letter.DeadlineInDays - daysAdvanced;
            if (daysRemaining <= 0 && letter.DeadlineInDays > 0)
            {
                // This letter would expire
                impacts.Add(new AffectedLetterInfo
                {
                    LetterId = letter.Id,
                    Route = $"{letter.SenderName} → {letter.RecipientName}",
                    CurrentDaysRemaining = letter.DeadlineInDays,
                    ResultDaysRemaining = daysRemaining
                });
            }
        }
        
        return impacts;
    }
    
    private TimeBlocks GetTimeBlockForHour(int hour)
    {
        return hour switch
        {
            >= 6 and < 8 => TimeBlocks.Dawn,
            >= 8 and < 12 => TimeBlocks.Morning,
            >= 12 and < 16 => TimeBlocks.Afternoon,
            >= 16 and < 20 => TimeBlocks.Evening,
            >= 20 and < 22 => TimeBlocks.Night,
            _ => TimeBlocks.LateNight
        };
    }
}

public class TimeImpactInfo
{
    public int Hours { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public TimeBlocks ResultTimeBlock { get; set; }
    public bool WouldAdvanceDay { get; set; }
    public int DaysAdvanced { get; set; }
    public int ActiveHoursRemaining { get; set; }
    public int LettersExpiring { get; set; }
    public List<AffectedLetterInfo> AffectedLetters { get; set; }
}

public class AffectedLetterInfo
{
    public string LetterId { get; set; }
    public string Route { get; set; }
    public int CurrentDaysRemaining { get; set; }
    public int ResultDaysRemaining { get; set; }
}