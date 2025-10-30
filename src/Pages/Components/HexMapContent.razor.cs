using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// DUMB DISPLAY COMPONENT - NO BUSINESS LOGIC
    /// Displays hex grid map with terrain colors, player position, and location markers
    /// All data comes from GameWorld (single source of truth)
    /// </summary>
    public class HexMapContentBase : ComponentBase
    {
        [Inject] protected GameWorld GameWorld { get; set; }

        [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

        protected List<Hex> Hexes { get; set; } = new();
        protected List<Location> Locations { get; set; } = new();
        protected AxialCoordinates PlayerPosition { get; set; }

        // CSS hex size (width of hexagon)
        protected const double HexSize = 40;

        protected override async Task OnInitializedAsync()
        {
            LoadMapData();
            await Task.CompletedTask;
        }

        protected override async Task OnParametersSetAsync()
        {
            LoadMapData();
            await Task.CompletedTask;
        }

        /// <summary>
        /// Load map data from GameWorld (single source of truth)
        /// </summary>
        private void LoadMapData()
        {
            Hexes = GameWorld.WorldHexGrid.Hexes;
            Locations = GameWorld.Locations.Where(loc => loc.HexPosition != null).ToList();

            Player player = GameWorld.GetPlayer();
            if (player?.CurrentPosition != null)
            {
                PlayerPosition = player.CurrentPosition;
            }
        }

        /// <summary>
        /// Get CSS position for hex (left, top in pixels)
        /// Uses pointy-top orientation with proper tessellation
        /// </summary>
        protected (int left, int top) GetHexPosition(AxialCoordinates coords)
        {
            // CSS hex dimensions (must match CSS .hex width/height)
            double hexWidth = 70;   // matches CSS .hex width
            double hexHeight = 80;  // matches CSS .hex height

            // Pointy-top hex tessellation formula
            // Hexes should have gaps between them, so add spacing beyond tessellation
            double horizontalSpacing = hexWidth + 5;
            double verticalSpacing = hexHeight * 0.75 + 5;

            // Calculate position with proper offset for interlocking pattern
            // For pointy-top: x offset by half spacing per row, y advances by 3/4 height
            double x = coords.Q * horizontalSpacing + (coords.R * (horizontalSpacing * 0.5));
            double y = coords.R * verticalSpacing;

            // Center the grid in viewport
            int left = (int)(x + 500);
            int top = (int)(y + 350);

            return (left, top);
        }

        /// <summary>
        /// Get terrain color for hex (CSS class name)
        /// </summary>
        protected string GetTerrainColorClass(Hex hex)
        {
            return hex.Terrain.ToString().ToLower();
        }

        /// <summary>
        /// Check if coordinates match player position
        /// </summary>
        protected bool IsPlayerPosition(AxialCoordinates coords)
        {
            return coords.Q == PlayerPosition.Q && coords.R == PlayerPosition.R;
        }

        /// <summary>
        /// Get location at specific coordinates (if any)
        /// </summary>
        protected Location GetLocationAtCoordinates(AxialCoordinates coords)
        {
            return Locations.FirstOrDefault(loc =>
                loc.HexPosition != null &&
                loc.HexPosition.Value.Q == coords.Q &&
                loc.HexPosition.Value.R == coords.R);
        }

        /// <summary>
        /// Close map and return to previous screen
        /// </summary>
        protected async Task CloseMap()
        {
            await GameScreen.NavigateToScreen(ScreenMode.Location);
        }
    }
}
