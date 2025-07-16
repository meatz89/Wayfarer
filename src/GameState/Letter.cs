using System;
using Wayfarer.GameState;

namespace Wayfarer.GameState
{
    public enum LetterSize
    {
        Small,    // Quick note, easy to carry
        Medium,   // Standard letter
        Large     // Package or bulky correspondence
    }

    public class Letter
    {
        public string Id { get; set; }
        public string SenderName { get; set; }  // Just a string for minimal POC
        public string RecipientName { get; set; }
        public int Deadline { get; set; }
        public int Payment { get; set; }
        public ConnectionType TokenType { get; set; }
        
        // Additional properties for future use but set defaults for POC
        public int QueuePosition { get; set; } = 0;
        public LetterSize Size { get; set; } = LetterSize.Medium;
        public bool IsFromPatron { get; set; } = false;
        
        // Helper properties
        public bool IsExpired => Deadline <= 0;
        
        public string GetDeadlineDescription()
        {
            if (IsExpired) return "EXPIRED";
            if (Deadline == 1) return "1 day (URGENT!)";
            if (Deadline <= 3) return $"{Deadline} days (urgent)";
            return $"{Deadline} days";
        }
        
        public string GetTokenTypeIcon()
        {
            return TokenType switch
            {
                ConnectionType.Trust => "❤️",
                ConnectionType.Trade => "🪙",
                ConnectionType.Noble => "👑",
                ConnectionType.Common => "🍺",
                ConnectionType.Shadow => "🌑",
                _ => "❓"
            };
        }
        
        public Letter()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
}