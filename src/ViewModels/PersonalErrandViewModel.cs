using System.Collections.Generic;

namespace Wayfarer.ViewModels
{
    public class PersonalErrandViewModel
    {
        public List<AvailableErrand> AvailableErrands { get; set; } = new();
        public List<CompletedErrand> CompletedErrands { get; set; } = new();
        public PlayerErrandStatus PlayerStatus { get; set; } = new();
    }

    public class AvailableErrand
    {
        public string NPCId { get; set; }
        public string NPCName { get; set; }
        public string NPCProfession { get; set; }
        public string LocationId { get; set; }
        public string LocationName { get; set; }
        public string Description { get; set; }
        public bool CanPerform { get; set; }
        public string BlockingReason { get; set; }
        
        // Requirements
        public int RequiredTokens { get; set; } = 2;
        public int PlayerTokens { get; set; }
        public bool RequiresMedicine { get; set; } = true;
        public bool HasMedicine { get; set; }
        public int StaminaCost { get; set; } = 2;
        public int TimeCost { get; set; } = 2;
        
        // Rewards
        public ConnectionType TokenReward { get; set; } = ConnectionType.Trust;
        public string ContextualDescription { get; set; }
    }

    public class CompletedErrand
    {
        public string NPCName { get; set; }
        public string Description { get; set; }
        public int DayCompleted { get; set; }
        public ConnectionType TokenEarned { get; set; }
    }

    public class PlayerErrandStatus
    {
        public int CurrentStamina { get; set; }
        public int MedicineCount { get; set; }
        public List<string> MedicineItems { get; set; } = new();
        public bool CanDoErrands => CurrentStamina >= 2 && MedicineCount > 0;
    }
}