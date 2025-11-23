using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Landing view - High-level navigation hub showing 3-5 main choices.
    /// Entry point for all location navigation.
    /// </summary>
    public class LandingBase : ComponentBase
    {
        [Parameter] public string CurrentLocationName { get; set; }
        [Parameter] public List<LocationActionViewModel> TravelActions { get; set; } = new();
        [Parameter] public List<LocationActionViewModel> LocationSpecificActions { get; set; } = new();
        [Parameter] public List<LocationActionViewModel> PlayerActions { get; set; } = new();

        [Parameter] public EventCallback<LocationViewState> OnNavigateToView { get; set; }
        [Parameter] public EventCallback<LocationActionViewModel> OnExecuteLocationAction { get; set; }

        /// <summary>
        /// Get CSS class for action based on type
        /// </summary>
        protected string GetActionClass(LocationActionViewModel action)
        {
            return action.ActionType switch
            {
                "rest" => "rest",
                "secureroom" => "lodging",
                "work" => "work",
                _ => ""
            };
        }
    }
}
