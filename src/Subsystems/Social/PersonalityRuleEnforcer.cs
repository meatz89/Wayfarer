/// <summary>
/// Enforces personality-specific conversation rules during gameplay (Updated for Initiative System)
/// Tracks state, validates plays, and modifies outcomes based on NPC personality
/// INITIATIVE SYSTEM: All Focus references updated to Initiative
/// </summary>
public class PersonalityRuleEnforcer
{
    private readonly PersonalityModifier _modifier;
    private readonly List<int> _playedInitiativeThisTurn;
    private CardInstance _lastPlayedCard;
    private int _highestInitiativeThisTurn;
    private bool _isFirstCardOfTurn;

    public PersonalityRuleEnforcer(PersonalityModifier modifier)
    {
        _modifier = modifier ?? new PersonalityModifier { Type = PersonalityModifierType.None };
        _playedInitiativeThisTurn = new List<int>();
        _isFirstCardOfTurn = true;
        _highestInitiativeThisTurn = 0;
    }

    /// <summary>
    /// Validate if a card can be legally played according to personality rules (UPDATED FOR INITIATIVE)
    /// Proud: Cards must be played in ascending Initiative order (not Focus)
    /// </summary>
    public bool ValidatePlay(CardInstance card, out string violationMessage)
    {
        violationMessage = "";

        if (_modifier.Type == PersonalityModifierType.AscendingFocusRequired)
        {
            // Proud: Cards must be played in ascending Initiative order (UPDATED: Focus → Initiative)
            int cardInitiative = GetCardInitiativeCost(card);
            if (_playedInitiativeThisTurn.Any() && cardInitiative <= _playedInitiativeThisTurn.Last())
            {
                violationMessage = "Proud NPCs require cards played in ascending Initiative order within each turn";
                return false;
            }
        }

        // All other personality types allow any play order
        return true;
    }

    // MERCANTILE bonus now implemented in ModifyMomentumChange() as +3 Momentum

    /// <summary>
    /// Get Initiative cost for a card
    /// </summary>
    private int GetCardInitiativeCost(CardInstance card)
    {
        return card.SocialCardTemplate.InitiativeCost;
    }

    /// <summary>
    /// Modify momentum change based on personality rules (UPDATED FOR 4-RESOURCE SYSTEM)
    /// Mercantile: Highest Initiative card gains +3 Momentum
    /// Devoted: Momentum losses increased by flat amount (additive penalty)
    /// Cunning: Playing same Initiative as previous card costs -2 momentum
    /// Steadfast: All momentum changes capped at ±2
    /// </summary>
    public int ModifyMomentumChange(CardInstance card, int baseMomentumChange)
    {
        int modifiedChange = baseMomentumChange;

        switch (_modifier.Type)
        {
            case PersonalityModifierType.HighestFocusBonus:
                // Mercantile: Highest Initiative card gains +3 Momentum
                int cardInitiative = GetCardInitiativeCost(card);
                if (_isFirstCardOfTurn || cardInitiative > _highestInitiativeThisTurn)
                {
                    // DOMAIN COLLECTION PRINCIPLE: Use GetParameter helper (returns 0 if not found)
                    int momentumBonus = _modifier.GetParameter("momentumBonus");
                    if (momentumBonus == 0) momentumBonus = 3; // Default value
                    modifiedChange += momentumBonus;
                }
                break;

            case PersonalityModifierType.MomentumLossIncreased:
                // Devoted: When momentum would decrease, add additional loss (additive, not multiplicative)
                if (baseMomentumChange < 0)
                {
                    // DOMAIN COLLECTION PRINCIPLE: Use GetParameter helper (returns 0 if not found)
                    int additionalLoss = _modifier.GetParameter("additionalLoss");
                    if (additionalLoss == 0) additionalLoss = 2; // Default value
                    modifiedChange -= additionalLoss; // Subtract additional loss (making it more negative)
                }
                break;

            case PersonalityModifierType.RepeatFocusPenalty:
                // Cunning: Playing same Initiative as previous card costs -2 momentum
                int cunningCardInitiative = GetCardInitiativeCost(card);
                int lastInitiative = _lastPlayedCard != null ? GetCardInitiativeCost(_lastPlayedCard) : -1;
                if (lastInitiative != -1 && cunningCardInitiative == lastInitiative)
                {
                    // DOMAIN COLLECTION PRINCIPLE: Use GetParameter helper (returns 0 if not found)
                    int penalty = _modifier.GetParameter("penalty");
                    if (penalty == 0) penalty = -2; // Default value
                    modifiedChange += penalty;
                }
                break;

            case PersonalityModifierType.RapportChangeCap:
                // Steadfast: All momentum changes capped at ±2
                // DOMAIN COLLECTION PRINCIPLE: Use GetParameter helper (returns 0 if not found)
                int cap = _modifier.GetParameter("cap");
                if (cap == 0) cap = 2; // Default value
                modifiedChange = Math.Max(-cap, Math.Min(cap, modifiedChange));
                break;
        }

        return modifiedChange;
    }

    /// <summary>
    /// Get additional doubt for Devoted personality on card failure (additive)
    /// </summary>
    public int GetDoubtModifier()
    {
        if (_modifier.Type == PersonalityModifierType.MomentumLossIncreased)
        {
            // Devoted: +1 additional doubt on failure (additive penalty)
            return 1; // Additional doubt beyond the base +1
        }

        return 0; // No additional doubt
    }

    /// <summary>
    /// Record that a card was played (for state tracking) - UPDATED FOR INITIATIVE
    /// </summary>
    public void OnCardPlayed(CardInstance card)
    {
        int cardInitiative = GetCardInitiativeCost(card);
        _playedInitiativeThisTurn.Add(cardInitiative); _lastPlayedCard = card;

        if (_isFirstCardOfTurn || cardInitiative > _highestInitiativeThisTurn)
        {
            _highestInitiativeThisTurn = cardInitiative;
        }

        _isFirstCardOfTurn = false;
    }

    /// <summary>
    /// Reset state when LISTEN action occurs - UPDATED FOR INITIATIVE
    /// </summary>
    public void OnListen()
    {
        _playedInitiativeThisTurn.Clear(); _isFirstCardOfTurn = true;
        _highestInitiativeThisTurn = 0;         // Note: We keep _lastPlayedCard for Cunning personality check across turns
    }

    /// <summary>
    /// Get a description of the active personality rule
    /// </summary>
    public string GetRuleDescription()
    {
        return _modifier.Type switch
        {
            PersonalityModifierType.AscendingFocusRequired => "Proud: Cards must be played in ascending Initiative order",
            PersonalityModifierType.MomentumLossIncreased => "Devoted: All momentum losses +2 penalty, +1 doubt on failure",
            PersonalityModifierType.HighestFocusBonus => "Mercantile: Highest Initiative card gains +3 Momentum",
            PersonalityModifierType.RepeatFocusPenalty => "Cunning: Repeat Initiative costs -2 Momentum",
            PersonalityModifierType.RapportChangeCap => "Steadfast: All Momentum changes capped at ±2",
            _ => ""
        };
    }

    /// <summary>
    /// Check if a specific card would get the Mercantile bonus - UPDATED FOR INITIATIVE
    /// </summary>
    public bool WouldGetMercantileBonus(CardInstance card)
    {
        if (_modifier.Type != PersonalityModifierType.HighestFocusBonus)
            return false;

        int cardInitiative = GetCardInitiativeCost(card);
        return _isFirstCardOfTurn || cardInitiative > _highestInitiativeThisTurn;
    }

    /// <summary>
    /// Check if a specific card would trigger Cunning penalty - UPDATED FOR INITIATIVE
    /// </summary>
    public bool WouldTriggerCunningPenalty(CardInstance card)
    {
        if (_modifier.Type != PersonalityModifierType.RepeatFocusPenalty)
            return false;

        int cardInitiative = GetCardInitiativeCost(card);
        int lastInitiative = _lastPlayedCard != null ? GetCardInitiativeCost(_lastPlayedCard) : -1;
        return lastInitiative != -1 && cardInitiative == lastInitiative;
    }

    /// <summary>
    /// Get momentum cost for LISTEN action based on personality rules
    /// Most personalities have no additional cost, but some may modify it
    /// </summary>
    public int GetListenMomentumCost()
    {
        // For now, most personalities don't modify LISTEN cost
        // Future implementations could add personality-specific LISTEN costs
        return 0; // Default: no momentum cost for LISTEN
    }

    /// <summary>
    /// Check if momentum losses should be increased for Devoted personality
    /// </summary>
    public bool ShouldIncreaseMomentumLoss()
    {
        return _modifier.Type == PersonalityModifierType.MomentumLossIncreased;
    }

    /// <summary>
    /// Get the current state for UI display - UPDATED FOR INITIATIVE
    /// </summary>
    public PersonalityRuleState GetCurrentState()
    {
        return new PersonalityRuleState
        {
            ModifierType = _modifier.Type,
            PlayedInitiativeOrder = _playedInitiativeThisTurn.ToList(),
            LastPlayedInitiative = _lastPlayedCard != null ? GetCardInitiativeCost(_lastPlayedCard) : null,
            HighestInitiativeThisTurn = _highestInitiativeThisTurn,
            IsFirstCardOfTurn = _isFirstCardOfTurn
        };
    }
}

/// <summary>
/// State information for UI display - UPDATED FOR INITIATIVE SYSTEM
/// </summary>
public class PersonalityRuleState
{
    public PersonalityModifierType ModifierType { get; set; }
    public List<int> PlayedInitiativeOrder { get; set; }
    public int? LastPlayedInitiative { get; set; }
    public int HighestInitiativeThisTurn { get; set; }
    public bool IsFirstCardOfTurn { get; set; }

}