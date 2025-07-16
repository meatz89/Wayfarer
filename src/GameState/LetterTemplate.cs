using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    public class LetterTemplate
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public ConnectionType TokenType { get; set; }
        public int MinDeadline { get; set; }
        public int MaxDeadline { get; set; }
        public int MinPayment { get; set; }
        public int MaxPayment { get; set; }
        
        // Optional fields for future expansion
        public string[] PossibleSenders { get; set; } // NPCs who can send this type
        public string[] PossibleRecipients { get; set; } // NPCs who can receive this type
    }
}