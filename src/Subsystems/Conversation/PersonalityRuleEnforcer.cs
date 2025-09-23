using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Enforces personality-specific conversation rules during gameplay
/// Tracks state, validates plays, and modifies outcomes based on NPC personality
/// </summary>
public class PersonalityRuleEnforcer
{
    private readonly PersonalityModifier _modifier;
    private readonly List<int> _playedFocusThisTurn;
    private CardInstance _lastPlayedCard;
    private int _highestFocusThisTurn;
    private bool _isFirstCardOfTurn;

    public PersonalityRuleEnforcer(PersonalityModifier modifier)
    {
        _modifier = modifier ?? new PersonalityModifier { Type = PersonalityModifierType.None };
        _playedFocusThisTurn = new List<int>();
        _isFirstCardOfTurn = true;
        _highestFocusThisTurn = 0;
    }

    /// <summary>
    /// Validate if a card can be legally played according to personality rules
    /// </summary>
    public bool ValidatePlay(CardInstance card, out string violationMessage)
    {
        violationMessage = null;

        if (_modifier.Type == PersonalityModifierType.AscendingFocusRequired)
        {
            // Proud: Cards must be played in ascending focus order
            if (_playedFocusThisTurn.Any() && card.Focus <= _playedFocusThisTurn.Last())
            {
                violationMessage = "Proud NPCs require cards played in ascending focus order within each turn";
                return false;
            }
        }

        // All other personality types allow any play order
        return true;
    }

    /// <summary>
    /// Modify the success rate of a card based on personality rules
    /// </summary>
    public int ModifySuccessRate(CardInstance card, int baseSuccessRate)
    {
        if (_modifier.Type == PersonalityModifierType.HighestFocusBonus)
        {
            // Mercantile: Highest focus card each turn gains +30% success
            if (_isFirstCardOfTurn || card.Focus > _highestFocusThisTurn)
            {
                int bonusPercent = _modifier.Parameters.ContainsKey("bonusPercent")
                    ? _modifier.Parameters["bonusPercent"]
                    : 30;
                return Math.Min(100, baseSuccessRate + bonusPercent);
            }
        }

        return baseSuccessRate;
    }

    /// <summary>
    /// Modify rapport change based on personality rules
    /// </summary>
    public int ModifyRapportChange(CardInstance card, int baseRapportChange)
    {
        int modifiedChange = baseRapportChange;

        switch (_modifier.Type)
        {
            case PersonalityModifierType.MomentumLossDoubled:
                // Devoted: When momentum would decrease, decrease it twice
                if (baseRapportChange < 0)
                {
                    int multiplier = _modifier.Parameters.ContainsKey("multiplier")
                        ? _modifier.Parameters["multiplier"]
                        : 2;
                    modifiedChange = baseRapportChange * multiplier;
                }
                break;

            case PersonalityModifierType.RepeatFocusPenalty:
                // Cunning: Playing same focus as previous card costs -2 rapport
                if (_lastPlayedCard != null && card.Focus == _lastPlayedCard.Focus)
                {
                    int penalty = _modifier.Parameters.ContainsKey("penalty")
                        ? _modifier.Parameters["penalty"]
                        : -2;
                    modifiedChange += penalty;
                }
                break;

            case PersonalityModifierType.RapportChangeCap:
                // Steadfast: All rapport changes capped at ±2
                int cap = _modifier.Parameters.ContainsKey("cap")
                    ? _modifier.Parameters["cap"]
                    : 2;
                modifiedChange = Math.Max(-cap, Math.Min(cap, modifiedChange));
                break;
        }

        return modifiedChange;
    }

    /// <summary>
    /// Record that a card was played (for state tracking)
    /// </summary>
    public void OnCardPlayed(CardInstance card)
    {
        _playedFocusThisTurn.Add(card.Focus);
        _lastPlayedCard = card;

        if (_isFirstCardOfTurn || card.Focus > _highestFocusThisTurn)
        {
            _highestFocusThisTurn = card.Focus;
        }

        _isFirstCardOfTurn = false;
    }

    /// <summary>
    /// Reset state when LISTEN action occurs
    /// </summary>
    public void OnListen()
    {
        _playedFocusThisTurn.Clear();
        _isFirstCardOfTurn = true;
        _highestFocusThisTurn = 0;
        // Note: We keep _lastPlayedCard for Cunning personality check across turns
    }

    /// <summary>
    /// Get a description of the active personality rule
    /// </summary>
    public string GetRuleDescription()
    {
        return _modifier.Type switch
        {
            PersonalityModifierType.AscendingFocusRequired => "Proud: Cards must be played in ascending focus order",
            PersonalityModifierType.MomentumLossDoubled => "Devoted: All momentum losses doubled",
            PersonalityModifierType.HighestFocusBonus => "Mercantile: Your highest focus card gains +30% success",
            PersonalityModifierType.RepeatFocusPenalty => "Cunning: Playing same focus as previous costs -2 rapport",
            PersonalityModifierType.RapportChangeCap => "Steadfast: All rapport changes capped at ±2",
            _ => ""
        };
    }

    /// <summary>
    /// Check if a specific card would get the Mercantile bonus
    /// </summary>
    public bool WouldGetMercantileBonus(CardInstance card)
    {
        if (_modifier.Type != PersonalityModifierType.HighestFocusBonus)
            return false;

        return _isFirstCardOfTurn || card.Focus > _highestFocusThisTurn;
    }

    /// <summary>
    /// Check if a specific card would trigger Cunning penalty
    /// </summary>
    public bool WouldTriggerCunningPenalty(CardInstance card)
    {
        if (_modifier.Type != PersonalityModifierType.RepeatFocusPenalty)
            return false;

        return _lastPlayedCard != null && card.Focus == _lastPlayedCard.Focus;
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
    /// Get the current state for UI display
    /// </summary>
    public PersonalityRuleState GetCurrentState()
    {
        return new PersonalityRuleState
        {
            ModifierType = _modifier.Type,
            PlayedFocusOrder = _playedFocusThisTurn.ToList(),
            LastPlayedFocus = _lastPlayedCard?.Focus,
            HighestFocusThisTurn = _highestFocusThisTurn,
            IsFirstCardOfTurn = _isFirstCardOfTurn
        };
    }
}

/// <summary>
/// State information for UI display
/// </summary>
public class PersonalityRuleState
{
    public PersonalityModifierType ModifierType { get; set; }
    public List<int> PlayedFocusOrder { get; set; }
    public int? LastPlayedFocus { get; set; }
    public int HighestFocusThisTurn { get; set; }
    public bool IsFirstCardOfTurn { get; set; }
}