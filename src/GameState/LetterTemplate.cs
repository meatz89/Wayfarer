public enum LetterCategory
{
    Basic,    // 3-4 tokens required
    Quality,  // 5-7 tokens required  
    Premium   // 8+ tokens required
}

public class LetterTemplate
{
        public string Id { get; set; }
        public string Description { get; set; }
        public ConnectionType TokenType { get; set; }
        public int MinDeadline { get; set; }
        public int MaxDeadline { get; set; }
        public int MinPayment { get; set; }
        public int MaxPayment { get; set; }
        
        // Letter category and requirements
        public LetterCategory Category { get; set; } = LetterCategory.Basic;
        public int MinTokensRequired { get; set; } = 3; // Minimum tokens with NPC to unlock this template
        
        // Optional fields for future expansion
        public string[] PossibleSenders { get; set; } // NPCs who can send this type
        public string[] PossibleRecipients { get; set; } // NPCs who can receive this type
        
        // Letter chain properties
        public string[] UnlocksLetterIds { get; set; } = new string[0]; // Letter templates unlocked by delivering this letter
        public bool IsChainLetter { get; set; } = false;
    }