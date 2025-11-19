
/// <summary>
/// Parser for Obligation definitions - converts DTOs to domain models
/// Creates Obligation entities with phase definitions for situation spawning
/// Validates challenge type IDs against GameWorld at parse time
/// </summary>
public class ObligationParser
{
    private readonly GameWorld _gameWorld;

    public ObligationParser(GameWorld gameWorld)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
    }

    public Obligation ParseObligation(ObligationDTO dto)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidDataException("Obligation missing required field 'Id'");
        if (string.IsNullOrEmpty(dto.Name))
            throw new InvalidDataException($"Obligation '{dto.Id}' missing required field 'Name'");
        if (string.IsNullOrEmpty(dto.Description))
            throw new InvalidDataException($"Obligation '{dto.Id}' missing required field 'Description'");

        return new Obligation
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            IntroAction = ParseIntroAction(dto.Intro), // Returns null if dto.Intro is null
            ColorCode = dto.ColorCode,
            ObligationType = ParseObligationType(dto.ObligationType), // Handles null with default
            PatronNpcId = dto.PatronNpcId,
            DeadlineSegment = dto.DeadlineSegment,
            CompletionRewardCoins = dto.CompletionRewardCoins,
            CompletionRewardItems = dto.CompletionRewardItems, // DTO has inline init, trust it
            CompletionRewardXP = ParseXPRewards(dto.CompletionRewardXP), // Handles null internally
            SpawnedObligationIds = dto.SpawnedObligationIds, // DTO has inline init, trust it
            PhaseDefinitions = dto.Phases.Select((p, index) => ParsePhaseDefinition(p, dto.Id)).ToList() // DTO has inline init, trust it
        };
    }

    private ObligationPhaseDefinition ParsePhaseDefinition(ObligationPhaseDTO dto, string obligationId)
    {
        return new ObligationPhaseDefinition
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            OutcomeNarrative = dto.OutcomeNarrative,
            // SituationRequirements system eliminated - phases progress through actual situation completion tracking
            CompletionReward = ParseCompletionReward(dto.CompletionReward)
        };
    }

    private PhaseCompletionReward ParseCompletionReward(PhaseCompletionRewardDTO dto)
    {
        if (dto == null) return null; // Optional reward - valid to be null

        PhaseCompletionReward reward = new PhaseCompletionReward
        {
            UnderstandingReward = dto.UnderstandingReward
        };

        // Parse scene spawns using Scene-Situation template architecture
        // Convert SceneSpawnInfoDTO → SceneSpawnReward (spawned scene uses SceneTemplate.PlacementFilter)
        if (dto.ScenesSpawned != null)
        {
            foreach (SceneSpawnInfoDTO spawnDto in dto.ScenesSpawned)
            {
                throw new NotImplementedException(
                    "Obligation scene spawning requires refactoring to use categorical PlacementFilter. " +
                    "SceneSpawnInfoDTO.TargetType/TargetEntityId (legacy absolute placement) must be replaced with " +
                    "full PlacementFilterDTO containing categorical properties (LocationTypes, PersonalityTypes, etc.). " +
                    "Scenes will reuse entities naturally through categorical matching - no concrete IDs needed.");
            }
        }

        return reward;
    }

    private void ValidateDeckId(string deckId, TacticalSystemType systemType, string phaseId)
    {
        if (string.IsNullOrEmpty(deckId))
        {
            throw new InvalidDataException($"Obligation intro action '{phaseId}' has no deckId specified");
        }

        bool exists = systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.Any(d => d.Id == deckId),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.Any(d => d.Id == deckId),
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.Any(d => d.Id == deckId),
            _ => throw new InvalidDataException($"Unknown TacticalSystemType: {systemType}")
        };

        if (!exists)
        {
            throw new InvalidDataException(
                $"Obligation intro action '{phaseId}' references {systemType} engagement deck '{deckId}' which does not exist in GameWorld. " +
                $"Available {systemType} engagement decks: {string.Join(", ", GetAvailableChallengeDeckIds(systemType))}"
            );
        }
    }

    private IEnumerable<string> GetAvailableChallengeDeckIds(TacticalSystemType systemType)
    {
        return systemType switch
        {
            TacticalSystemType.Mental => _gameWorld.MentalChallengeDecks.Select(d => d.Id),
            TacticalSystemType.Physical => _gameWorld.PhysicalChallengeDecks.Select(d => d.Id),
            TacticalSystemType.Social => _gameWorld.SocialChallengeDecks.Select(d => d.Id),
            _ => new List<string>()
        };
    }

    private TacticalSystemType ParseSystemType(string systemTypeString)
    {
        // Optional field - defaults to Mental if missing/invalid
        return Enum.TryParse<TacticalSystemType>(systemTypeString, out TacticalSystemType type)
            ? type
            : TacticalSystemType.Mental;
    }

    private ObligationIntroAction ParseIntroAction(ObligationIntroActionDTO dto)
    {
        if (dto == null) return null; // Optional intro action - valid to be null

        // HIGHLANDER: Resolve LocationId from DTO to Location object
        Location location = null;
        if (!string.IsNullOrEmpty(dto.LocationId))
        {
            location = _gameWorld.GetLocation(dto.LocationId);
            if (location == null)
                throw new InvalidDataException($"ObligationIntroAction references unknown location: '{dto.LocationId}'");
        }

        return new ObligationIntroAction
        {
            TriggerType = ParseTriggerType(dto.TriggerType), // Handles null with default
            TriggerPrerequisites = ParseObligationPrerequisites(dto.TriggerPrerequisites), // Handles null
            ActionText = dto.ActionText,
            Location = location, // Object reference, not string ID
            IntroNarrative = dto.IntroNarrative,
            CompletionReward = ParseCompletionReward(dto.CompletionReward) // Handles null
        };
    }

    private DiscoveryTriggerType ParseTriggerType(string triggerTypeString)
    {
        // Optional field - defaults to ImmediateVisibility if missing/invalid
        if (string.IsNullOrEmpty(triggerTypeString))
            return DiscoveryTriggerType.ImmediateVisibility;

        return Enum.TryParse<DiscoveryTriggerType>(triggerTypeString, out DiscoveryTriggerType type)
            ? type
            : DiscoveryTriggerType.ImmediateVisibility;
    }

    private ObligationObligationType ParseObligationType(string typeString)
    {
        // Optional field - defaults to SelfDiscovered if missing/invalid
        if (string.IsNullOrEmpty(typeString))
            return ObligationObligationType.SelfDiscovered;

        return Enum.TryParse<ObligationObligationType>(typeString, out ObligationObligationType type)
            ? type
            : ObligationObligationType.SelfDiscovered;
    }

    private ObligationPrerequisites ParseObligationPrerequisites(ObligationPrerequisitesDTO dto)
    {
        if (dto == null) return new ObligationPrerequisites(); // Optional prerequisites - return empty if null

        // HIGHLANDER: Resolve LocationId from DTO to Location object
        Location location = null;
        if (!string.IsNullOrEmpty(dto.LocationId))
        {
            location = _gameWorld.GetLocation(dto.LocationId);
            if (location == null)
                throw new InvalidDataException($"ObligationPrerequisites references unknown location: '{dto.LocationId}'");
        }

        return new ObligationPrerequisites
        {
            Location = location // Object reference, not string ID
        };
    }

    private List<StatXPReward> ParseXPRewards(Dictionary<string, int> xpRewardsDto)
    {
        // DTO has inline init - trust it, but might be empty
        if (xpRewardsDto.Count == 0)
            return new List<StatXPReward>();

        List<StatXPReward> rewards = new List<StatXPReward>();

        foreach (KeyValuePair<string, int> entry in xpRewardsDto)
        {
            PlayerStatType statType = ParsePlayerStatType(entry.Key);
            if (statType != PlayerStatType.None && entry.Value > 0)
            {
                rewards.Add(new StatXPReward
                {
                    Stat = statType,
                    XPAmount = entry.Value
                });
            }
        }

        return rewards;
    }

    private PlayerStatType ParsePlayerStatType(string statName)
    {
        // Optional field - returns None if missing/invalid
        if (string.IsNullOrEmpty(statName))
            return PlayerStatType.None;

        return Enum.TryParse<PlayerStatType>(statName, ignoreCase: true, out PlayerStatType statType)
            ? statType
            : PlayerStatType.None;
    }
}
