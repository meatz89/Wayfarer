using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Wayfarer.Pages.Components
{
    /// <summary>
    /// Data structure for stat display information
    /// </summary>
    public class StatDisplayInfo
    {
        public PlayerStatType StatType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Level { get; set; }
        public int CurrentXP { get; set; }
        public int XPToNext { get; set; }
        public int ProgressPercent { get; set; }
        public int SuccessBonus { get; set; }
        public bool HasPersistenceBonus { get; set; }
    }

    public class PlayerStatsDisplayBase : ComponentBase
    {
        [Inject] protected GameFacade GameFacade { get; set; }

        [Parameter] public bool ShowTotalProgress { get; set; } = true;

        protected List<StatDisplayInfo> StatDisplayInfos { get; set; } = new();
        protected int TotalLevel { get; set; }
        protected int TotalXP { get; set; }
        protected string PrimaryStatName { get; set; } = "";

        protected override void OnInitialized()
        {
            RefreshStatData();
        }

        protected void RefreshStatData()
        {
            PlayerStats playerStats = GameFacade.GetPlayerStats();

            StatDisplayInfos = new List<StatDisplayInfo>();

            foreach (PlayerStatType statType in Enum.GetValues<PlayerStatType>())
            {
                int level = playerStats.GetLevel(statType);
                int currentXP = playerStats.GetXP(statType);
                int xpToNext = playerStats.GetXPToNextLevel(statType);
                int progressPercent = playerStats.GetProgressPercent(statType);
                int successBonus = playerStats.GetSuccessBonus(statType);

                StatDisplayInfos.Add(new StatDisplayInfo
                {
                    StatType = statType,
                    Name = GetStatDisplayName(statType),
                    Description = GetStatDescription(statType),
                    Level = level,
                    CurrentXP = currentXP,
                    XPToNext = level >= 5 ? 0 : xpToNext,
                    ProgressPercent = progressPercent,
                    SuccessBonus = successBonus,
                    HasPersistenceBonus = playerStats.HasPersistenceBonus(statType)
                });
            }

            // Sort by level descending, then by XP descending
            StatDisplayInfos = StatDisplayInfos
                .OrderByDescending(s => s.Level)
                .ThenByDescending(s => s.CurrentXP)
                .ToList();

            TotalLevel = playerStats.GetTotalLevel();
            TotalXP = playerStats.GetTotalXP();
            // DELETED: PrimaryStat concept - stats don't have global "primary" role
            PrimaryStatName = "N/A"; // Stats are equal, no "primary" stat
        }

        protected string GetStatDisplayName(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Commerce => "Commerce",
                PlayerStatType.Cunning => "Cunning",
                _ => statType.ToString()
            };
        }

        protected string GetStatDescription(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.Insight => "Analytical intelligence and observation",
                PlayerStatType.Rapport => "Emotional intelligence and empathy",
                PlayerStatType.Authority => "Leadership and persuasion",
                PlayerStatType.Commerce => "Negotiation and trade",
                PlayerStatType.Cunning => "Subtlety and indirection",
                _ => ""
            };
        }

        protected string GetStatIconClass(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.Insight => "insight",
                PlayerStatType.Rapport => "rapport",
                PlayerStatType.Authority => "authority",
                PlayerStatType.Commerce => "commerce",
                PlayerStatType.Cunning => "cunning",
                _ => "default"
            };
        }

        /// <summary>
        /// Call this method to refresh the display when stats change
        /// </summary>
        public void UpdateDisplay()
        {
            RefreshStatData();
            StateHasChanged();
        }
    }
}