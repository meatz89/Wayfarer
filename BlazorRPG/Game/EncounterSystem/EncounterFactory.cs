public class EncounterFactory
{
    public EncounterTemplate GetDefaultEncounterTemplate()
    {
        return new EncounterTemplate()
        {
            Name = "Default Template",
            Duration = 4,
            MaxPressure = 12,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = Encounter.HostilityLevels.Neutral,

            EncounterNarrativeTags = new List<NarrativeTag>
            {
                NarrativeTagRepository.DistractingCommotion,
                NarrativeTagRepository.UnsteadyConditions
            },
        };
    }

    /// <summary>
    /// Creates the encounter for the given location
    /// </summary>
    public Encounter CreateEncounterFromTemplate(
        EncounterTemplate template,
        Location location,
        LocationSpot locationSpot,
        EncounterTypes EncounterType)
    {
        Encounter encounter = new Encounter(
            location.Name,
            locationSpot.Name,
            template.Duration,
            template.MaxPressure,
            template.PartialThreshold, template.StandardThreshold, template.ExceptionalThreshold, // Momentum thresholds: 12+ (Partial), 16+ (Standard), 20+ (Exceptional)
            template.Hostility,
            EncounterType);

        encounter.SetDifficulty(location.Difficulty);

        foreach (NarrativeTag narrativeTag in template.EncounterNarrativeTags)
        {
            encounter.AddTag(narrativeTag);
        }

        List<StrategicTag> tags = CreateStrategicTags(locationSpot);
        foreach (StrategicTag strategicTag in tags)
        {
            encounter.AddTag(strategicTag);
        }
        return encounter;
    }

    private List<StrategicTag> CreateStrategicTags(LocationSpot locationSpot)
    {
        List<StrategicTag> strategicTags =
        [
            .. AddIlluminationStrategicTags(locationSpot.Illumination),
            .. AddPopulationStrategicTags(locationSpot.Population),
            .. AddAtmosphereStrategicTags(locationSpot.Atmosphere),
            .. AddPhysicalStrategicTags(locationSpot.Physical),
        ];

        return strategicTags;
    }

    private List<StrategicTag> AddIlluminationStrategicTags(Illumination illumination)
    {
        List<StrategicTag> strategicTags = new List<StrategicTag>();
        if (illumination.Equals(Illumination.Bright))
        {
            strategicTags.Add(new StrategicTag(
                "Clear Visibility",
                illumination,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { illumination },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Precision
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Exposed Position",
                illumination,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { illumination },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Concealment
                )
            ));
        }
        else if (illumination.Equals(Illumination.Roguey))
        {
            strategicTags.Add(new StrategicTag(
                "Partial Cover",
                illumination,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { illumination },
                    StrategicTagEffectType.DecreasePressure,
                    ApproachTags.Concealment
                )
            ));
        }
        else if (illumination.Equals(Illumination.Dark))
        {
            strategicTags.Add(new StrategicTag(
                "Concealing Darkness",
                illumination,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { illumination },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Concealment
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Limited Visibility",
                illumination,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { illumination },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Precision
                )
            ));
        }

        return strategicTags;
    }

    private List<StrategicTag> AddPopulationStrategicTags(Population population)
    {
        List<StrategicTag> strategicTags = new List<StrategicTag>();
        if (population.Equals(Population.Crowded))
        {
            strategicTags.Add(new StrategicTag(
                "Rapport Pressure",
                population,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { population },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Rapport
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Public Scrutiny",
                population,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { population },
                    StrategicTagEffectType.IncreasePressure,
                    ApproachTags.Concealment
                )
            ));
        }
        else if (population.Equals(Population.Quiet))
        {
            strategicTags.Add(new StrategicTag(
                "Focused Attention",
                population,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { population },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Analysis
                )
            ));
        }
        else if (population.Equals(Population.Scholarly))
        {
            strategicTags.Add(new StrategicTag(
                "Scholarly Focus",
                population,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { population },
                    StrategicTagEffectType.DecreasePressure,
                    ApproachTags.Analysis
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Careful Deliberation",
                population,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { population },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Dominance
                )
            ));
        }

        return strategicTags;
    }

    private List<StrategicTag> AddAtmosphereStrategicTags(Atmosphere atmosphere)
    {
        List<StrategicTag> strategicTags = new List<StrategicTag>();
        if (atmosphere.Equals(Atmosphere.Tense))
        {
            strategicTags.Add(new StrategicTag(
                "Structured Environment",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Precision
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Rapport Protocol",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Dominance
                )
            ));
        }
        else if (atmosphere.Equals(Atmosphere.Chaotic))
        {
            strategicTags.Add(new StrategicTag(
                "Unpredictable Situation",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.IncreasePressure,
                    ApproachTags.Precision
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Opportunity in Chaos",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Concealment
                )
            ));
        }
        else if (atmosphere.Equals(Atmosphere.Rough))
        {
            strategicTags.Add(new StrategicTag(
                "Intimidating Presence",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Dominance
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Tense Atmosphere",
                atmosphere,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { atmosphere },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Rapport
                )
            ));
        }

        return strategicTags;
    }

    private List<StrategicTag> AddPhysicalStrategicTags(Physical physical)
    {
        List<StrategicTag> strategicTags = new List<StrategicTag>();
        if (physical.Equals(Physical.Confined))
        {
            strategicTags.Add(new StrategicTag(
                "Limited Movement",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.DecreaseMomentum,
                    ApproachTags.Dominance
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Close Quarters",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Precision
                )
            ));
        }
        else if (physical.Equals(Physical.Expansive))
        {
            strategicTags.Add(new StrategicTag(
                "Room to Maneuver",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.IncreaseMomentum,
                    ApproachTags.Dominance
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Many Hiding Places",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.DecreasePressure,
                    ApproachTags.Concealment
                )
            ));
        }
        else if (physical.Equals(Physical.Hazardous))
        {
            strategicTags.Add(new StrategicTag(
                "Dangerous Terrain",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.IncreasePressure,
                    ApproachTags.Dominance
                )
            ));

            strategicTags.Add(new StrategicTag(
                "Careful Navigation",
                physical,
                new EnvironmentalPropertyEffect(
                    new List<IEnvironmentalProperty> { physical },
                    StrategicTagEffectType.DecreasePressure,
                    ApproachTags.Precision
                )
            ));
        }

        return strategicTags;
    }
}
