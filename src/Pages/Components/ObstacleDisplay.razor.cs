using Microsoft.AspNetCore.Components;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Component for displaying obstacle properties and status
    /// Shows the 5 universal properties, cleared status, and permanence flag
    /// </summary>
    public class ObstacleDisplayBase : ComponentBase
    {
        [Parameter] public Obstacle Obstacle { get; set; }

        /// <summary>
        /// Get CSS class for obstacle severity based on total magnitude
        /// </summary>
        protected string GetSeverityClass()
        {
            if (Obstacle == null) return "low";

            int magnitude = Obstacle.GetTotalMagnitude();

            if (magnitude <= 0) return "cleared";
            if (magnitude <= 5) return "low";
            if (magnitude <= 10) return "moderate";
            if (magnitude <= 20) return "high";
            return "extreme";
        }

        /// <summary>
        /// Get display color for property values
        /// Returns CSS class based on property value
        /// </summary>
        protected string GetPropertyClass(int value)
        {
            if (value <= 0) return "cleared";
            if (value <= 2) return "low";
            if (value <= 5) return "moderate";
            if (value <= 8) return "high";
            return "extreme";
        }

        /// <summary>
        /// Get display text for permanence status
        /// </summary>
        protected string GetPermanenceDisplay()
        {
            if (Obstacle == null) return "";
            return Obstacle.IsPermanent ? "Permanent (persists when cleared)" : "Temporary (removed when cleared)";
        }

        /// <summary>
        /// Check if obstacle has any non-zero properties
        /// </summary>
        protected bool HasActiveProperties()
        {
            if (Obstacle == null) return false;
            return Obstacle.PhysicalDanger > 0 ||
                   Obstacle.MentalComplexity > 0 ||
                   Obstacle.SocialDifficulty > 0;
        }
    }
}
