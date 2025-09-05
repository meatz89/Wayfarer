using System.Collections.Generic;

namespace Wayfarer
{
    /// <summary>
    /// Container for all narrative and dialogue data used in conversations.
    /// </summary>
    public class SystemNarratives
    {
        public ConversationNarratives conversationNarratives { get; set; }
        public SystemMessages systemMessages { get; set; }
        public Dictionary<string, string> observationCardNames { get; set; }
        public Dictionary<string, string> observationCardDialogues { get; set; }
        public string burdenCardDialogue { get; set; }
        public Dictionary<string, string> exchangeCardDialogues { get; set; }
        public Dictionary<string, string> flowCardDialogues { get; set; }
        public Dictionary<string, string> tokenCardDialogues { get; set; }
        public string letterCardDialogue { get; set; }
        public string defaultCardDialogue { get; set; }
    }
}