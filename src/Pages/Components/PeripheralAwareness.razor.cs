using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

public class PeripheralAwarenessBase : ComponentBase
{
    [Parameter] public List<string> Observations { get; set; } = new();
    [Parameter] public string DeadlineNarrative { get; set; }
    [Parameter] public string QueuePressureNarrative { get; set; }
    [Parameter] public List<string> EnvironmentalHints { get; set; } = new();
    
    [Inject] private GameFacade GameFacade { get; set; }
    
    protected bool HasContent => HasDeadlinePressure || HasEnvironmentalHints || HasQueuePressure || Observations?.Any() == true;
    protected bool HasDeadlinePressure => !string.IsNullOrEmpty(DeadlineNarrative);
    protected bool HasQueuePressure => !string.IsNullOrEmpty(QueuePressureNarrative);
    protected bool HasEnvironmentalHints => EnvironmentalHints?.Any() == true;
    
    protected override void OnInitialized()
    {
        base.OnInitialized();
        UpdatePeripheralInfo();
    }
    
    private void UpdatePeripheralInfo()
    {
        if (GameFacade == null) return;
        
        // Get the actual letter queue from the player
        var player = GameFacade.GetPlayer();
        if (player == null) return;
        
        var letterQueue = player.LetterQueue;
        
        // Get deadline narrative for most urgent letter
        var urgentLetter = letterQueue
            .Where(l => l != null && l.State == LetterState.Accepted)
            .OrderBy(l => l.DeadlineInHours)
            .FirstOrDefault();
            
        if (urgentLetter != null && urgentLetter.DeadlineInHours <= 72) // 3 days = 72 hours
        {
            DeadlineNarrative = urgentLetter.DeadlineInHours switch
            {
                <= 6 => $"⚡ {urgentLetter.SenderName}'s letter burns in your satchel - {urgentLetter.DeadlineInHours}h left!",
                <= 24 => $"⏰ {urgentLetter.SenderName} needs their letter delivered today - {urgentLetter.DeadlineInHours}h",
                <= 48 => $"📬 {urgentLetter.SenderName}'s letter needs attention soon",
                _ => $"📬 {urgentLetter.SenderName}'s letter weighs on your mind"
            };
        }
        
        // Calculate queue pressure
        var activeLetterCount = letterQueue.Count(l => l != null && l.State == LetterState.Accepted);
        if (activeLetterCount > 6)
        {
            QueuePressureNarrative = "Your satchel strains with accumulated correspondence";
        }
        else if (urgentLetter != null && urgentLetter.DeadlineInHours <= 24) // 1 day = 24 hours
        {
            QueuePressureNarrative = $"⚡ {urgentLetter.SenderName}'s letter burns in your satchel - {urgentLetter.DeadlineInHours}h left!";
        }
    }
}