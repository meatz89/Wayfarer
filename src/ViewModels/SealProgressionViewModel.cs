using System.Collections.Generic;

namespace Wayfarer.ViewModels
{
    public class SealProgressionViewModel
    {
        public List<OwnedSeal> OwnedSeals { get; set; } = new();
        public List<WornSeal> WornSeals { get; set; } = new();
        public List<EndorsementProgress> EndorsementTracking { get; set; } = new();
        public int MaxWornSeals { get; set; } = 3;
    }
    
    public class OwnedSeal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SealType Type { get; set; }
        public SealTier Tier { get; set; }
        public string Description { get; set; }
        public string Material { get; set; }
        public string Insignia { get; set; }
        public int DayIssued { get; set; }
        public string IssuingGuild { get; set; }
        public bool IsWorn { get; set; }
        public List<string> Benefits { get; set; } = new();
    }
    
    public class WornSeal
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SealType Type { get; set; }
        public SealTier Tier { get; set; }
        public int SlotNumber { get; set; } // 1-3
    }
    
    public class EndorsementProgress
    {
        public SealType Type { get; set; }
        public string TypeName { get; set; }
        public int CurrentEndorsements { get; set; }
        public SealTier CurrentTier { get; set; }
        public SealTier? NextTier { get; set; }
        public int? EndorsementsToNext { get; set; }
        public string NextTierBenefits { get; set; }
        public List<string> GuildLocations { get; set; } = new();
    }
}