using System;
using System.Collections.Generic;
using System.Linq;

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

    // DELETED: ModifySuccessRate() - Success rates don't exist in deterministic card system
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
    /// Devoted: Momentum losses doubled
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
                    int momentumBonus = _modifier.Parameters.ContainsKey("momentumBonus")
                        ? _modifier.Parameters["momentumBonus"]
                        : 3;
                    modifiedChange += momentumBonus;
                }
                break;

            case PersonalityModifierType.MomentumLossDoubled:
                // Devoted: When momentum would decrease, decrease it twice
                if (baseMomentumChange < 0)
                {
                    int multiplier = _modifier.Parameters.ContainsKey("multiplier")
                        ? _modifier.Parameters["multiplier"]
                        : 2;
                    modifiedChange = baseMomentumChange * multiplier;
                }
                break;

            case PersonalityModifierType.RepeatFocusPenalty:
                // Cunning: Playing same Initiative as previous card costs -2 momentum
                int cunningCardInitiative = GetCardInitiativeCost(card);
                int lastInitiative = _lastPlayedCard != null ? GetCardInitiativeCost(_lastPlayedCard) : -1;
                if (lastInitiative != -1 && cunningCardInitiative == lastInitiative)
                {
                    int penalty = _modifier.Parameters.ContainsKey("penalty")
                        ? _modifier.Parameters["penalty"]
                        : -2;
                    modifiedChange += penalty;
                }
                break;

            case PersonalityModifierType.RapportChangeCap:
                // Steadfast: All momentum changes capped at ±2
                int cap = _modifier.Parameters.ContainsKey("cap")
                    ? _modifier.Parameters["cap"]
                    : 2;
                modifiedChange = Math.Max(-cap, Math.Min(cap, modifiedChange));
                break;
        }

        return modifiedChange;
    }

    /// <summary>
    /// Check if doubt should be doubled for Devoted personality on card failure
    /// </summary>
    public int GetDoubtModifier()
    {
        if (_modifier.Type == PersonalityModifierType.MomentumLossDoubled)
        {
            // Devoted: +1 additional doubt on failure (doubles the base +1 doubt)
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
            PersonalityModifierType.MomentumLossDoubled => "Devoted: All momentum losses doubled, +1 doubt on failure",
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
    /// Check if momentum losses should be doubled for Devoted personality
    /// </summary>
    public bool ShouldDoubleMomentumLoss()
    {
        return _modifier.Type == PersonalityModifierType.MomentumLossDoubled;
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