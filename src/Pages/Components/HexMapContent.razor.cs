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
        protected List<Venue> Venues { get; set; } = new();
        protected Dictionary<AxialCoordinates, Venue> HexToVenueMap { get; set; } = new();

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
            Venues = GameWorld.Venues;

            Player player = GameWorld.GetPlayer();
            if (player?.CurrentPosition != null)
            {
                PlayerPosition = player.CurrentPosition;
            }

            BuildHexToVenueMap();
        }

        /// <summary>
        /// Build mapping from hex coordinates to venue for border detection
        /// </summary>
        private void BuildHexToVenueMap()
        {
            HexToVenueMap.Clear();
            foreach (Location location in Locations)
            {
                if (location.HexPosition.HasValue && location.Venue != null)
                {
                    HexToVenueMap[location.HexPosition.Value] = location.Venue;
                }
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

        /// <summary>
        /// Get venue at hex coordinates (if any location at that hex belongs to a venue)
        /// </summary>
        protected Venue GetVenueAtCoordinates(AxialCoordinates coords)
        {
            if (HexToVenueMap.TryGetValue(coords, out Venue venue))
            {
                return venue;
            }
            return null;
        }

        /// <summary>
        /// Get CSS class for venue border edges. Returns classes for each edge that borders a different venue.
        /// Pointy-top hex has 6 edges: N, NE, SE, S, SW, NW
        /// </summary>
        protected string GetVenueBorderClasses(AxialCoordinates coords)
        {
            Venue currentVenue = GetVenueAtCoordinates(coords);
            if (currentVenue == null)
                return "";

            List<string> borderClasses = new();

            // Define 6 neighbor directions for pointy-top hex grid (axial coordinates)
            AxialCoordinates[] neighbors = new AxialCoordinates[]
            {
                new AxialCoordinates(coords.Q, coords.R - 1),      // N
                new AxialCoordinates(coords.Q + 1, coords.R - 1),  // NE
                new AxialCoordinates(coords.Q + 1, coords.R),      // SE
                new AxialCoordinates(coords.Q, coords.R + 1),      // S
                new AxialCoordinates(coords.Q - 1, coords.R + 1),  // SW
                new AxialCoordinates(coords.Q - 1, coords.R)       // NW
            };

            string[] edgeNames = new[] { "n", "ne", "se", "s", "sw", "nw" };

            for (int i = 0; i < 6; i++)
            {
                Venue neighborVenue = GetVenueAtCoordinates(neighbors[i]);
                if (neighborVenue == null || neighborVenue != currentVenue)
                {
                    borderClasses.Add($"venue-border-{edgeNames[i]}");
                }
            }

            return string.Join(" ", borderClasses);
        }

        /// <summary>
        /// Check if this hex is the "center" of a venue (first location in venue alphabetically)
        /// Used to display venue name label only once per venue
        /// </summary>
        protected bool IsVenueLabelHex(AxialCoordinates coords)
        {
            Venue venue = GetVenueAtCoordinates(coords);
            if (venue == null) return false;

            // Find the first hex (by coordinates) that belongs to this venue to display the label
            Location firstLocation = Locations
                .Where(l => l.Venue == venue && l.HexPosition.HasValue)
                .OrderBy(l => l.HexPosition.Value.R)
                .ThenBy(l => l.HexPosition.Value.Q)
                .FirstOrDefault();

            if (firstLocation == null) return false;

            return firstLocation.HexPosition.Value.Q == coords.Q &&
                   firstLocation.HexPosition.Value.R == coords.R;
        }

        /// <summary>
        /// Get consistent color for venue (based on hash of venue name)
        /// </summary>
        protected string GetVenueColorClass(Venue venue)
        {
            if (venue == null) return "";
            int hash = Math.Abs(venue.Name.GetHashCode()) % 6;
            return $"venue-color-{hash}";
        }
    }
}
