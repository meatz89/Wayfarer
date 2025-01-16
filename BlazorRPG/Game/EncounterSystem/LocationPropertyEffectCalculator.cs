public class LocationPropertyEffectCalculator
{
    public void ApplyPropertyEffects(EncounterChoice choice, EncounterContext context)
    {
        //// Iterate through all defined effects
        //foreach (LocationPropertyChoiceEffect effect in LocationPropertyChoiceEffects.AllEffects)
        //{
        //    // Check if the effect's location property matches the current location's properties
        //    if (LocationPropertyMatches(effect.LocationProperty, context.LocationProperties))
        //    {
        //        // Apply the effect using the ValueTransformation
        //        effect.ValueEffect.Apply(choice, context);
        //    }
        //}
    }

    public int CalculateEnergyCostModifier(EncounterChoice choice, LocationProperties properties)
    {
        int modifier = 0;

        //// Iterate through all defined effects
        //foreach (LocationPropertyChoiceEffect effect in LocationPropertyChoiceEffects.AllEffects)
        //{
        //    // Check if the effect's location property matches the current location's properties
        //    if (LocationPropertyMatches(effect.LocationProperty, properties))
        //    {
        //        // Apply energy modification if applicable
        //        if (effect.ValueEffect is EnergyModification energyMod && energyMod.TargetArchetype == choice.Archetype)
        //        {
        //            modifier += energyMod.EnergyCostModifier;
        //        }
        //    }
        //}

        return modifier;
    }

    public List<Requirement> CalculateLocationRequirements(EncounterChoice choice, LocationProperties properties)
    {
        List<Requirement> requirements = new();

        //// Iterate through all defined effects
        //foreach (LocationPropertyChoiceEffect effect in LocationPropertyChoiceEffects.AllEffects)
        //{
        //    // Check if the effect's location property matches the current location's properties
        //    if (LocationPropertyMatches(effect.LocationProperty, properties))
        //    {
        //        // Apply requirement modifications if applicable
        //        if (effect.ValueEffect is RequirementModification requirementMod)
        //        {
        //            // Assuming you have a way to create Requirement objects based on the modification
        //            // This is a placeholder, adjust according to your Requirement creation logic
        //            Requirement newRequirement = CreateRequirementFromModification(requirementMod);
        //            if (newRequirement != null)
        //            {
        //                requirements.Add(newRequirement);
        //            }
        //        }
        //    }
        //}

        return requirements;
    }


    public List<Outcome> CalculatePropertyCosts(EncounterChoice choice, LocationProperties properties)
    {
        List<Outcome> costs = new();

        //// Iterate through all defined effects
        //foreach (LocationPropertyChoiceEffect effect in LocationPropertyChoiceEffects.AllEffects)
        //{
        //    // Check if the effect's location property matches the current location's properties
        //    if (LocationPropertyMatches(effect.LocationProperty, properties))
        //    {
        //        // Apply cost modifications if applicable
        //        if (effect.ValueEffect is OutcomeModification outcomeMod)
        //        {
        //            // Assuming you have a way to create Outcome objects based on the modification
        //            // This is a placeholder, adjust according to your Outcome creation logic
        //            Outcome newOutcome = CreateOutcomeFromModification(outcomeMod);
        //            if (newOutcome != null)
        //            {
        //                costs.Add(newOutcome);
        //            }
        //        }
        //    }
        //}

        return costs;
    }

    public List<Outcome> CalculatePropertyRewards(EncounterChoice choice, LocationProperties properties)
    {
        List<Outcome> rewards = new();

        //// Iterate through all defined effects
        //foreach (LocationPropertyChoiceEffect effect in LocationPropertyChoiceEffects.AllEffects)
        //{
        //    // Check if the effect's location property matches the current location's properties
        //    if (LocationPropertyMatches(effect.LocationProperty, properties))
        //    {
        //        // Apply reward modifications if applicable
        //        if (effect.ValueEffect is OutcomeModification outcomeMod)
        //        {
        //            // Assuming you have a way to create Outcome objects based on the modification
        //            // This is a placeholder, adjust according to your Outcome creation logic
        //            Outcome newOutcome = CreateOutcomeFromModification(outcomeMod);
        //            if (newOutcome != null)
        //            {
        //                rewards.Add(newOutcome);
        //            }
        //        }
        //    }
        //}

        return rewards;
    }

    // Helper method to check if a LocationPropertyTypeValue matches the current location's properties
    private bool LocationPropertyMatches(LocationPropertyTypeValue locationProperty, LocationProperties properties)
    {
        if (locationProperty is AtmosphereValue atmosphereValue)
        {
            return properties.Atmosphere.HasValue && properties.Atmosphere.Value == atmosphereValue.Atmosphere;
        }
        else if (locationProperty is SpaceValue spaceValue)
        {
            return properties.Space.HasValue && properties.Space.Value == spaceValue.Space;
        }
        else if (locationProperty is ActivityLevelValue activityLevelValue)
        {
            return properties.ActivityLevel.HasValue && properties.ActivityLevel.Value == activityLevelValue.ActivityLevel;
        }
        // Add other property type checks here

        return false;
    }

}