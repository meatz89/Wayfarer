using System;
using System.Collections.Generic;

namespace Wayfarer.ViewModels
{
    /// <summary>
    /// ViewModel for the conversation screen matching the mockup structure
    /// </summary>
    public class ConversationViewModel
    {
        // Core conversation data
        public string NpcName { get; set; }
        public string NpcId { get; set; }
        public string CurrentText { get; set; }
        public bool IsComplete { get; set; }
        public string ConversationTopic { get; set; }
        
        // Attention System
        public int CurrentAttention { get; set; }
        public int MaxAttention { get; set; } = 3;
        public string AttentionNarrative { get; set; }
        
        // Location Context
        public string LocationName { get; set; }
        public string LocationAtmosphere { get; set; }
        public List<string> LocationPath { get; set; } = new();
        
        // Character Focus
        public string CharacterName { get; set; }
        public string CharacterState { get; set; } // Body language description
        public Dictionary<string, string> RelationshipStatus { get; set; } = new();
        
        // Dialogue
        public List<string> SpokenWords { get; set; } = new();
        public string CharacterAction { get; set; } // Mid-dialogue action
        
        // Choices with Mechanics
        public List<ConversationChoiceViewModel> Choices { get; set; } = new();
        
        // Peripheral Awareness
        public string DeadlinePressure { get; set; }
        public List<string> EnvironmentalHints { get; set; } = new();
        public string BodyLanguageDescription { get; set; }
        public List<string> PeripheralObservations { get; set; } = new();
        public string InternalMonologue { get; set; }
        public List<BindingObligationViewModel> BindingObligations { get; set; } = new();
        
        // Bottom Status
        public string CurrentLocation { get; set; }
        public string QueueStatus { get; set; } // e.g., "3/8"
        public string CoinStatus { get; set; } // e.g., "12s"
        public string CurrentTime { get; set; } // e.g., "TUE 3:45 PM"
        
        // Context tags for narrative generation
        public List<string> PressureTags { get; set; } = new();
        public List<string> RelationshipTags { get; set; } = new();
        public List<string> FeelingTags { get; set; } = new();
        public List<string> DiscoveryTags { get; set; } = new();
        public List<string> ResourceTags { get; set; } = new();
        
        // Scene pressure metrics
        public int MinutesUntilDeadline { get; set; }
        public int LetterQueueSize { get; set; }
    }
    
    /// <summary>
    /// ViewModel for conversation choices with mechanical display
    /// </summary>
    public class ConversationChoiceViewModel
    {
        public string Id { get; set; }
        public string Text { get; set; } // The italicized thought text
        public int AttentionCost { get; set; }
        public string AttentionDisplay { get; set; } // "Free", "◆ 1", "◆◆ 2", etc.
        public bool IsLocked { get; set; }
        public List<MechanicEffectViewModel> Mechanics { get; set; } = new();
        
        // Additional properties for compatibility
        public bool IsAvailable { get; set; } = true;
        public string UnavailableReason { get; set; }
        public string AttentionDescription { get; set; }
        public bool IsInternalThought { get; set; }
        public string EmotionalTone { get; set; }
    }
    
    /// <summary>
    /// ViewModel for displaying mechanical effects of choices
    /// </summary>
    public class MechanicEffectViewModel
    {
        public string Icon { get; set; } // "→", "✓", "⚠", "ℹ", "⏱", "⛓"
        public string Description { get; set; }
        public MechanicEffectType Type { get; set; }
    }
    
    public enum MechanicEffectType
    {
        Neutral,
        Positive,
        Negative
    }
    
    /// <summary>
    /// ViewModel for location screens matching the mockup structure
    /// </summary>
    public class LocationScreenViewModel
    {
        // Header Bar
        public string CurrentTime { get; set; }
        public string DeadlineTimer { get; set; }
        
        // Location Info
        public List<string> LocationPath { get; set; } = new();
        public string LocationName { get; set; }
        // Tags removed - atmosphere now calculated from NPC presence
        
        // Atmosphere
        public string AtmosphereText { get; set; }
        
        // Actions
        public List<LocationActionViewModel> QuickActions { get; set; } = new();
        
        // NPCs Present
        public List<NPCPresenceViewModel> NPCsPresent { get; set; } = new();
        
        // Observations
        public string ObservationHeader { get; set; } = "YOU NOTICE:";
        public List<ObservationViewModel> Observations { get; set; } = new();
        
        // Movement Options
        public List<RouteOptionViewModel> Routes { get; set; } = new();
        
        // Travel Progress (if traveling)
        public TravelProgressViewModel TravelProgress { get; set; }
    }
    
    // LocationTagViewModel and LocationTagType removed - tags replaced with atmosphere calculation
    
    /// <summary>
    /// ViewModel for location actions (the grid cards)
    /// </summary>
    public class LocationActionViewModel
    {
        public string Icon { get; set; } // Emoji
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Cost { get; set; } // "FREE", "1c", "10m", etc.
    }
    
    /// <summary>
    /// ViewModel for NPCs at a location
    /// </summary>
    public class NPCPresenceViewModel
    {
        public string Name { get; set; }
        public string MoodEmoji { get; set; }
        public string Description { get; set; }
        public List<InteractionOptionViewModel> Interactions { get; set; } = new();
    }
    
    /// <summary>
    /// ViewModel for NPC interaction options
    /// </summary>
    public class InteractionOptionViewModel
    {
        public string Text { get; set; }
        public string Cost { get; set; } // Time or money
    }
    
    /// <summary>
    /// ViewModel for observations at a location
    /// </summary>
    public class ObservationViewModel
    {
        public string Icon { get; set; } // Emoji or "❓" for unknown
        public string Text { get; set; }
        public bool IsUnknown { get; set; }
    }
    
    /// <summary>
    /// ViewModel for route/movement options
    /// </summary>
    public class RouteOptionViewModel
    {
        public string Destination { get; set; }
        public string TravelTime { get; set; }
        public string Detail { get; set; } // e.g., "West side", "Uphill"
    }
    
    /// <summary>
    /// ViewModel for travel progress display
    /// </summary>
    public class TravelProgressViewModel
    {
        public string Title { get; set; }
        public int ProgressPercent { get; set; }
        public string TimeWalked { get; set; }
        public string TimeRemaining { get; set; }
    }
}