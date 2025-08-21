using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates categorical narrative data for location screens.
/// NO TEXT GENERATION - only categories, enums, and context objects.
/// Frontend components will map these to actual narrative prose.
/// </summary>
public class LocationNarrativeGenerator
{
    private readonly NPCRepository _npcRepository;
    private readonly NPCStateResolver _stateResolver;
    private readonly ObservationSystem _observationSystem;
    private readonly AtmosphereCalculator _atmosphereCalculator;
    private readonly ITimeManager _timeManager;
    private readonly WorldMemorySystem _worldMemory;

    public LocationNarrativeGenerator(
        NPCRepository npcRepository,
        NPCStateResolver stateResolver,
        ObservationSystem observationSystem,
        AtmosphereCalculator atmosphereCalculator,
        ITimeManager timeManager,
        WorldMemorySystem worldMemory)
    {
        _npcRepository = npcRepository;
        _stateResolver = stateResolver;
        _observationSystem = observationSystem;
        _atmosphereCalculator = atmosphereCalculator;
        _timeManager = timeManager;
        _worldMemory = worldMemory;
    }

    /// <summary>
    /// Generate categorical context for a location
    /// </summary>
    public LocationNarrativeContext GenerateContext(Location location, LocationSpot spot)
    {
        TimeBlocks currentTime = _timeManager.GetCurrentTimeBlock();
        
        // Get NPCs at location
        List<NPC> npcsPresent = spot != null ? 
            _npcRepository.GetNPCsForLocationSpotAndTime(spot.SpotID, currentTime) :
            new List<NPC>();
        
        // Generate NPC contexts
        List<NPCPresenceContext> npcContexts = GenerateNPCContexts(npcsPresent);
        
        // Calculate atmosphere from NPCs
        AtmosphereCategory atmosphere = CalculateAtmosphere(npcsPresent, location);
        
        // Get observations
        List<ObservableViewModel> observations = spot != null ?
            _observationSystem.GetObservations(spot.SpotID) :
            new List<ObservableViewModel>();
        
        // Determine location mood
        LocationMood mood = DetermineLocationMood(npcsPresent, currentTime, atmosphere);
        
        // Get recent events that affect atmosphere
        List<RecentEventCategory> recentEvents = GetRecentEventCategories();
        
        // Determine activity level
        ActivityLevel activity = DetermineActivityLevel(npcsPresent.Count, currentTime);
        
        return new LocationNarrativeContext
        {
            LocationName = location.Name,
            TimeOfDay = currentTime,
            Atmosphere = atmosphere,
            Mood = mood,
            ActivityLevel = activity,
            NPCContexts = npcContexts,
            ObservationCount = observations.Count,
            HasUrgentObservations = observations.Any(o => o.Type == ObservationType.Important),
            RecentEvents = recentEvents,
            SpotCharacter = LocationSpotCharacter.Generic // LocationSpot doesn't have Character property
        };
    }

    /// <summary>
    /// Generate context for NPCs at a location
    /// </summary>
    public List<NPCPresenceContext> GenerateNPCContexts(List<NPC> npcs)
    {
        var contexts = new List<NPCPresenceContext>();
        
        foreach (var npc in npcs)
        {
            NPCEmotionalState emotionalState = _stateResolver.CalculateState(npc);
            
            // Determine NPC's current activity
            NPCActivity activity = DetermineNPCActivity(npc, emotionalState);
            
            // Calculate body language category
            BodyLanguageCategory bodyLanguage = DetermineBodyLanguage(emotionalState, npc.PersonalityType);
            
            // Determine if NPC has urgent business
            UrgencyIndicator urgency = CalculateUrgency(emotionalState);
            
            contexts.Add(new NPCPresenceContext
            {
                NPCId = npc.ID,
                Name = npc.Name,
                EmotionalState = emotionalState,
                Personality = npc.PersonalityType,
                Activity = activity,
                BodyLanguage = bodyLanguage,
                Urgency = urgency,
                Profession = npc.Profession,
                IsApproachable = emotionalState != NPCEmotionalState.HOSTILE
            });
        }
        
        return contexts;
    }

    private AtmosphereCategory CalculateAtmosphere(List<NPC> npcs, Location location)
    {
        // Get base atmosphere from location tags
        AtmosphereCategory baseAtmosphere = GetBaseAtmosphereFromTags(location.DomainTags);
        
        // Modify based on NPC presence
        if (!npcs.Any())
            return AtmosphereCategory.Empty;
        
        // Check for tense NPCs
        bool hasTenseNPCs = npcs.Any(n => 
        {
            var state = _stateResolver.CalculateState(n);
            return state == NPCEmotionalState.DESPERATE || 
                   state == NPCEmotionalState.ANXIOUS ||
                   state == NPCEmotionalState.HOSTILE;
        });
        
        if (hasTenseNPCs)
            return AtmosphereCategory.Tense;
        
        // Many NPCs = busy
        if (npcs.Count >= 3)
            return AtmosphereCategory.Bustling;
        
        // Few NPCs = quiet
        if (npcs.Count == 1)
            return AtmosphereCategory.Quiet;
        
        return baseAtmosphere;
    }

    private AtmosphereCategory GetBaseAtmosphereFromTags(List<string> tags)
    {
        if (tags == null || !tags.Any())
            return AtmosphereCategory.Neutral;
            
        // Map domain tags to atmosphere
        if (tags.Contains("tavern") || tags.Contains("inn"))
            return AtmosphereCategory.Warm;
        if (tags.Contains("market") || tags.Contains("trade"))
            return AtmosphereCategory.Bustling;
        if (tags.Contains("noble") || tags.Contains("palace"))
            return AtmosphereCategory.Formal;
        if (tags.Contains("docks") || tags.Contains("harbor"))
            return AtmosphereCategory.Rough;
        if (tags.Contains("temple") || tags.Contains("church"))
            return AtmosphereCategory.Peaceful;
        if (tags.Contains("gate") || tags.Contains("guard"))
            return AtmosphereCategory.Watchful;
            
        return AtmosphereCategory.Neutral;
    }

    private LocationMood DetermineLocationMood(List<NPC> npcs, TimeBlocks time, AtmosphereCategory atmosphere)
    {
        // Time-based moods
        if (time == TimeBlocks.Dawn)
            return LocationMood.Awakening;
        
        if (time == TimeBlocks.LateNight)
            return LocationMood.Secretive;
        
        // Atmosphere-based moods
        if (atmosphere == AtmosphereCategory.Tense)
            return LocationMood.Uneasy;
        
        if (atmosphere == AtmosphereCategory.Bustling)
            return LocationMood.Lively;
        
        if (atmosphere == AtmosphereCategory.Empty)
            return LocationMood.Abandoned;
        
        // NPC-based moods
        if (npcs.Any(n => _stateResolver.CalculateState(n) == NPCEmotionalState.DESPERATE))
            return LocationMood.Urgent;
        
        return LocationMood.Neutral;
    }

    private NPCActivity DetermineNPCActivity(NPC npc, NPCEmotionalState state)
    {
        // Emotional state overrides profession
        if (state == NPCEmotionalState.DESPERATE)
            return NPCActivity.WaitingAnxiously;
        
        if (state == NPCEmotionalState.ANXIOUS)
            return NPCActivity.Pacing;
        
        if (state == NPCEmotionalState.HOSTILE)
            return NPCActivity.Glowering;
        
        // Profession-based activities
        return npc.Profession switch
        {
            Professions.Merchant => NPCActivity.CountingCoins,
            Professions.Soldier => NPCActivity.StandingWatch,
            Professions.Noble => NPCActivity.HoldingCourt,
            Professions.Scholar => NPCActivity.ReadingDocuments,
            // Professions.Laborer => NPCActivity.Working, // Laborer doesn't exist, covered by Soldier
            _ => NPCActivity.GoingAboutBusiness
        };
    }

    private BodyLanguageCategory DetermineBodyLanguage(NPCEmotionalState state, PersonalityType personality)
    {
        return (state, personality) switch
        {
            (NPCEmotionalState.DESPERATE, _) => BodyLanguageCategory.Frantic,
            (NPCEmotionalState.ANXIOUS, PersonalityType.DEVOTED) => BodyLanguageCategory.Worried,
            (NPCEmotionalState.ANXIOUS, PersonalityType.PROUD) => BodyLanguageCategory.Stiff,
            (NPCEmotionalState.HOSTILE, _) => BodyLanguageCategory.Aggressive,
            (NPCEmotionalState.CALCULATING, _) => BodyLanguageCategory.Measured,
            (NPCEmotionalState.WITHDRAWN, _) => BodyLanguageCategory.Closed,
            _ => BodyLanguageCategory.Neutral
        };
    }

    private UrgencyIndicator CalculateUrgency(NPCEmotionalState state)
    {
        return state switch
        {
            NPCEmotionalState.DESPERATE => UrgencyIndicator.Critical,
            NPCEmotionalState.ANXIOUS => UrgencyIndicator.Pressing,
            NPCEmotionalState.HOSTILE => UrgencyIndicator.Confrontational,
            _ => UrgencyIndicator.None
        };
    }

    private ActivityLevel DetermineActivityLevel(int npcCount, TimeBlocks time)
    {
        // Night times are always quiet
        if (time == TimeBlocks.LateNight || time == TimeBlocks.Dawn)
            return ActivityLevel.Quiet;
        
        // Based on NPC count
        return npcCount switch
        {
            0 => ActivityLevel.Empty,
            1 => ActivityLevel.Quiet,
            2 => ActivityLevel.Moderate,
            >= 3 => ActivityLevel.Busy,
            _ => ActivityLevel.Moderate
        };
    }

    private List<RecentEventCategory> GetRecentEventCategories()
    {
        var categories = new List<RecentEventCategory>();
        
        if (_worldMemory != null)
        {
            WorldEvent recentEvent = _worldMemory.GetMostRecentEvent();
            if (recentEvent != null && (DateTime.Now - recentEvent.Timestamp).TotalMinutes < 30)
            {
                categories.Add(MapEventToCategory(recentEvent.Type));
            }
        }
        
        return categories;
    }

    private RecentEventCategory MapEventToCategory(WorldEventType eventType)
    {
        return eventType switch
        {
            WorldEventType.LetterDelivered => RecentEventCategory.Success,
            WorldEventType.DeadlineMissed => RecentEventCategory.Failure,
            WorldEventType.ObligationFulfilled => RecentEventCategory.Trust,
            WorldEventType.ConfrontationOccurred => RecentEventCategory.Conflict,
            _ => RecentEventCategory.Neutral
        };
    }
}

/// <summary>
/// Context for generating location narrative (categories only, no text)
/// </summary>
public class LocationNarrativeContext
{
    public string LocationName { get; set; }
    public TimeBlocks TimeOfDay { get; set; }
    public AtmosphereCategory Atmosphere { get; set; }
    public LocationMood Mood { get; set; }
    public ActivityLevel ActivityLevel { get; set; }
    public List<NPCPresenceContext> NPCContexts { get; set; }
    public int ObservationCount { get; set; }
    public bool HasUrgentObservations { get; set; }
    public List<RecentEventCategory> RecentEvents { get; set; }
    public LocationSpotCharacter SpotCharacter { get; set; }
}

/// <summary>
/// Context for NPC presence at a location
/// </summary>
public class NPCPresenceContext
{
    public string NPCId { get; set; }
    public string Name { get; set; }
    public NPCEmotionalState EmotionalState { get; set; }
    public PersonalityType Personality { get; set; }
    public NPCActivity Activity { get; set; }
    public BodyLanguageCategory BodyLanguage { get; set; }
    public UrgencyIndicator Urgency { get; set; }
    public Professions Profession { get; set; }
    public bool IsApproachable { get; set; }
}

// Categorical enums (NO text values)
public enum AtmosphereCategory 
{ 
    Neutral, Empty, Quiet, Warm, Bustling, Tense, 
    Formal, Rough, Peaceful, Watchful 
}

public enum LocationMood 
{ 
    Neutral, Awakening, Lively, Uneasy, Abandoned, 
    Urgent, Secretive 
}

public enum ActivityLevel { Empty, Quiet, Moderate, Busy }

public enum NPCActivity 
{ 
    GoingAboutBusiness, WaitingAnxiously, Pacing, Glowering,
    CountingCoins, StandingWatch, HoldingCourt, ReadingDocuments,
    Working, Drinking, Conversing, Observing
}

public enum BodyLanguageCategory 
{ 
    Neutral, Frantic, Worried, Stiff, Aggressive, 
    Measured, Closed, Open, Relaxed 
}

public enum UrgencyIndicator { None, Pressing, Critical, Confrontational }

public enum RecentEventCategory { Neutral, Success, Failure, Trust, Conflict }

public enum LocationSpotCharacter 
{ 
    Generic, Cozy, Grand, Shadowy, Open, Cramped, 
    Official, Informal 
}