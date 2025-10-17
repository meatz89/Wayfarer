using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Landing view - High-level navigation hub showing 3-5 main choices.
    /// Entry point for all location navigation.
    /// </summary>
    public class LandingBase : ComponentBase
    {
        [Parameter] public bool HasNPCs { get; set; }
        [Parameter] public bool HasLocationChallenges { get; set; }
        [Parameter] public bool HasSpots { get; set; }
        [Parameter] public string CurrentLocationName { get; set; }
        [Parameter] public List<LocationActionViewModel> TravelActions { get; set; } = new();

        [Parameter] public EventCallback<LocationViewState> OnNavigateToView { get; set; }
        [Parameter] public EventCallback<LocationActionViewModel> OnExecuteLocationAction { get; set; }
    }
}
