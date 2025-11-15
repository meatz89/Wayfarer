using Microsoft.AspNetCore.Components;

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
        protected string PrimaryStatName { get; set; }

        protected override void OnInitialized()
        {
            RefreshStatData();
        }

        protected void RefreshStatData()
        {
            Player player = GameFacade.GetPlayer();

            StatDisplayInfos = new List<StatDisplayInfo>
            {
                new StatDisplayInfo
                {
                    StatType = PlayerStatType.Insight,
                    Name = "Insight",
                    Description = GetStatDescription(PlayerStatType.Insight),
                    Level = player.Insight,
                    CurrentXP = 0,
                    XPToNext = 0,
                    ProgressPercent = 0,
                    SuccessBonus = 0,
                    HasPersistenceBonus = false
                },
                new StatDisplayInfo
                {
                    StatType = PlayerStatType.Rapport,
                    Name = "Rapport",
                    Description = GetStatDescription(PlayerStatType.Rapport),
                    Level = player.Rapport,
                    CurrentXP = 0,
                    XPToNext = 0,
                    ProgressPercent = 0,
                    SuccessBonus = 0,
                    HasPersistenceBonus = false
                },
                new StatDisplayInfo
                {
                    StatType = PlayerStatType.Authority,
                    Name = "Authority",
                    Description = GetStatDescription(PlayerStatType.Authority),
                    Level = player.Authority,
                    CurrentXP = 0,
                    XPToNext = 0,
                    ProgressPercent = 0,
                    SuccessBonus = 0,
                    HasPersistenceBonus = false
                },
                new StatDisplayInfo
                {
                    StatType = PlayerStatType.Diplomacy,
                    Name = "Diplomacy",
                    Description = GetStatDescription(PlayerStatType.Diplomacy),
                    Level = player.Diplomacy,
                    CurrentXP = 0,
                    XPToNext = 0,
                    ProgressPercent = 0,
                    SuccessBonus = 0,
                    HasPersistenceBonus = false
                },
                new StatDisplayInfo
                {
                    StatType = PlayerStatType.Cunning,
                    Name = "Cunning",
                    Description = GetStatDescription(PlayerStatType.Cunning),
                    Level = player.Cunning,
                    CurrentXP = 0,
                    XPToNext = 0,
                    ProgressPercent = 0,
                    SuccessBonus = 0,
                    HasPersistenceBonus = false
                }
            };

            // Sort by level descending
            StatDisplayInfos = StatDisplayInfos
                .OrderByDescending(s => s.Level)
                .ToList();

            TotalLevel = player.Insight + player.Rapport + player.Authority + player.Diplomacy + player.Cunning;
            TotalXP = 0; // No XP system anymore
            PrimaryStatName = "N/A"; // Stats are equal, no "primary" stat
        }

        protected string GetStatDisplayName(PlayerStatType statType)
        {
            return statType switch
            {
                PlayerStatType.Insight => "Insight",
                PlayerStatType.Rapport => "Rapport",
                PlayerStatType.Authority => "Authority",
                PlayerStatType.Diplomacy => "Diplomacy",
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
                PlayerStatType.Diplomacy => "Negotiation and trade",
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
                PlayerStatType.Diplomacy => "diplomacy",
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