using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Spots View - Shows all spots in current venue for movement between spots.
    /// </summary>
    public class SpotsViewBase : ComponentBase
    {
        [Parameter] public string CurrentLocationName { get; set; }
        [Parameter] public List<SpotWithNpcsViewModel> AvailableSpots { get; set; } = new();

        [Parameter] public EventCallback<string> OnMoveToSpot { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }
}
