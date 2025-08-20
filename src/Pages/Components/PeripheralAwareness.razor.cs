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
        Player player = GameFacade.GetPlayer();
        if (player == null) return;

        DeliveryObligation[] obligationQueue = player.ObligationQueue;

        // Get deadline narrative for most urgent obligation
        DeliveryObligation? urgentObligation = obligationQueue
            .Where(l => l != null && l.IsGenerated)
            .OrderBy(l => l.DeadlineInMinutes)
            .FirstOrDefault();

        if (urgentObligation != null && urgentObligation.DeadlineInMinutes <= 4320) // 3 days in minutes
        {
            int hoursLeft = urgentObligation.DeadlineInMinutes / 60;
            DeadlineNarrative = hoursLeft switch
            {
                <= 6 => $"⚡ {urgentObligation.SenderName}'s obligation burns urgent - {hoursLeft}h left!",
                <= 24 => $"⏰ {urgentObligation.SenderName} needs delivery today - {hoursLeft}h",
                <= 48 => $"📬 {urgentObligation.SenderName}'s obligation needs attention soon",
                _ => $"📬 {urgentObligation.SenderName}'s obligation weighs on your mind"
            };
        }

        // Calculate queue pressure
        int activeObligationCount = obligationQueue.Count(l => l != null && l.IsGenerated);
        if (activeObligationCount > 6)
        {
            QueuePressureNarrative = "Your satchel strains with accumulated correspondence";
        }
        else if (urgentObligation != null && urgentObligation.DeadlineInMinutes <= 1440) // 1 day in minutes
        {
            int hoursLeft = urgentObligation.DeadlineInMinutes / 60;
            QueuePressureNarrative = $"⚡ {urgentObligation.SenderName}'s obligation burns urgent - {hoursLeft}h left!";
        }
    }
}