public class ChoiceSetScore
{
    public List<ChoicePattern> ChoicePatterns { get; set; }
    public int BaseScore { get; set; }  // Starting point based on template

    // Situational Modifiers
    public int GetContextScore(EncounterActionContext context)
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
    //    if (values.Understanding >= 6 && this.UsesUnderstanding())
    //        score += 3;

    //    if (values.Connection >= 7 && this.UsesConnection())
    //        score += 3;

    //    // Penalize sets that might be dangerous
    //    if (values.Tension >= 7 && this.IncreaseTension())
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
    //    // - Lead to victory if advantage is high
    //    // - Reduce tension if it's dangerous
    //    // - Build needed resources if they're low
    //}

    //public bool UsesUnderstanding()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c => c.ValueType == ValueTypes.Understanding));
    //}

    //public bool UsesConnection()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c => c.ValueType == ValueTypes.Connection));
    //}

    //public bool IncreaseTension()
    //{
    //    return ChoicePatterns.Any(p =>
    //        p.BaseValueChanges.Any(c =>
    //            c.ValueType == ValueTypes.Tension && c.Change > 0));
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
