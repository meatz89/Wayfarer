using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Component for displaying obstacle properties and status
    /// Shows single Intensity value (1-3 scale), contexts, cleared status
    /// </summary>
    public class ObstacleDisplayBase : ComponentBase
    {
        [Parameter] public Obstacle Obstacle { get; set; }

        [Inject] protected ObstacleFacade ObstacleFacade { get; set; }

        protected ObstacleIntensity EffectiveIntensity { get; set; }
        protected List<EquipmentContextMatch> MatchingEquipment { get; set; } = new List<EquipmentContextMatch>();

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (Obstacle != null && ObstacleFacade != null)
            {
                EffectiveIntensity = ObstacleFacade.CalculateEffectiveIntensity(Obstacle.Id);
                MatchingEquipment = ObstacleFacade.FindMatchingEquipmentForObstacle(Obstacle.Id);
            }
        }

        /// <summary>
        /// Get CSS class for obstacle severity based on Intensity (1-3 scale)
        /// </summary>
        protected string GetSeverityClass()
        {
            if (Obstacle == null) return "low";

            int intensity = Obstacle.Intensity;

            if (intensity <= 0) return "cleared";
            if (intensity == 1) return "low";
            if (intensity == 2) return "moderate";
            return "high";
        }

        /// <summary>
        /// Get display color for Intensity value (1-3 scale)
        /// </summary>
        protected string GetPropertyClass(int value)
        {
            if (value <= 0) return "cleared";
            if (value == 1) return "low";
            if (value == 2) return "moderate";
            return "high";
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
        /// Check if obstacle has non-zero intensity
        /// </summary>
        protected bool HasActiveProperties()
        {
            if (Obstacle == null) return false;
            return Obstacle.Intensity > 0;
        }
    }
}
