public class ChoiceSetScore
{
    public List<ChoiceTemplate> ChoicePatterns { get; set; }
    public int BaseScore { get; set; }  // Starting point based on template

    // Situational Modifiers
    public int GetContextScore(EncounterContext context)
    {
        int score = BaseScore;

        /*

        // How well does this set use current narrative values?
        score += ScoreNarrativeValueUse(context.CurrentValues);

        // How appropriate is this set for current energy levels?
        score += ScoreEnergyState(context.PlayerState);

        // How well does this set leverage player's built resources?
        score += ScorePlayerResources(context.PlayerState);

        // How much does this set progress the encounter state?
        score += ScoreProgressPotential(context);

        */

        return score;
    }

    //public int ScoreNarrativeValueUse(EncounterStateValues values)
    //{
    //    int score = 0;

    //    // Reward sets that could use high values
    //    if (values.Insight >= 6 && this.UsesInsight())
    //        score += 3;

    //    if (values.Resonance >= 7 && this.UsesResonance())
    //        score += 3;

    //    // Penalize sets that might be dangerous
    //    if (values.Pressure >= 7 && this.IncreasePressure())
    //        score -= 2;

    //    return score;
    //}

    //public int ScoreEnergyState(PlayerState state)
    //{
    //    // Prefer sets that use player's highest energy type
    //    if (this.PrimaryEnergyType == GetHighestEnergyType(state))
    //        score += 2;

    //    // Avoid sets requiring depleted energy types
    //    if (GetEnergyLevel(state, this.PrimaryEnergyType) <= 2)
    //        score -= 3;

    //    return score;
    //}

    //public EnergyTypes GetHighestEnergyType(PlayerState state)
    //{
    //    if (state.PhysicalEnergy >= state.FocusEnergy &&
    //        state.PhysicalEnergy >= state.SocialEnergy)
    //        return EnergyTypes.Physical;
    //    if (state.FocusEnergy >= state.SocialEnergy)
    //        return EnergyTypes.Focus;
    //    return EnergyTypes.Social;
    //}

    //public int ScorePlayerResources(PlayerState state)
    //{
    //    // Reward sets that can use player's:
    //    // - Unlocked skills
    //    // - Acquired knowledge
    //    // - Built reputation
    //    // - Available items
    //}

    //public int ScoreProgressPotential(EncounterActionContext context)
    //{
    //    // Higher scores for sets that could:
    //    // - Lead to victory if outcome is high
    //    // - Reduce pressure if it's dangerous
    //    // - Build needed resources if they're low
    //}

    //public bool UsesInsight()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c => c.ValueType == ValueTypes.Insight));
    //}

    //public bool UsesResonance()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c => c.ValueType == ValueTypes.Resonance));
    //}

    //public bool IncreasePressure()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c =>
    //            c.ValueType == ValueTypes.Pressure && c.Change > 0));
    //}

    //public EnergyTypes GetDominantEnergy()
    //{
    //    IOrderedEnumerable<IGrouping<EnergyTypes, EnergyTypes>> energyTypes = ChoicePatterns
    //        .Select(p => p.EnergyType)
    //        .GroupBy(e => e)
    //        .OrderByDescending(g => g.Count());

    //    return energyTypes.First().Key;
    //}

    //public int GetEnergyLevel(PlayerState state, EnergyTypes type)
    //{
    //    return type switch
    //    {
    //        EnergyTypes.Physical => state.PhysicalEnergy,
    //        EnergyTypes.Social => state.SocialEnergy,
    //        EnergyTypes.Focus => state.FocusEnergy,
    //        _ => 0
    //    };
    //}
}
