using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// NPC List View - Shows all NPCs present at current spot.
    /// Click NPC to navigate to detailed view.
    /// </summary>
    public class NPCListViewBase : ComponentBase
    {
        [Parameter] public List<NpcViewModel> AvailableNpcs { get; set; } = new();

        [Parameter] public EventCallback<string> OnNavigateToNPC { get; set; }
        [Parameter] public EventCallback OnNavigateBack { get; set; }
    }
}
