using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Location Challenges View - Shows ambient Mental/Physical goals at location.
    /// Investigation goals are included as regular goals with InvestigationId property.
    /// </summary>
    public class LocationChallengesBase : ComponentBase
    {
        [Parameter] public List<GoalViewModel> MentalGoals { get; set; } = new();
        [Parameter] public List<GoalViewModel> PhysicalGoals { get; set; } = new();

        [Parameter] public EventCallback<string> OnNavigateToGoal { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }
}
