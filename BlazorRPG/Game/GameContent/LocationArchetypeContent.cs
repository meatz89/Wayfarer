public class ArchetypeEffect
{
    public Dictionary<ValueTypes, List<ValueTransformation>> ValueTransformations { get; set; } = new();
    public Dictionary<EnergyTypes, int> EnergyCostReductions { get; set; } = new();

}

public class LocationArchetypeContent
{
    public static Dictionary<LocationArchetypes, ArchetypeEffect> Effects { get; set; } = new()
    {
        {
            LocationArchetypes.Library, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Insight, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Pressure, TargetValue = ValueTypes.Insight, TransformationType = TransformationType.Convert }
                        }
                    },
                    {
                        ValueTypes.Resonance, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Resonance, TargetValue = ValueTypes.Insight, TransformationType = TransformationType.Set }
                        }
                    },
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Focus, 1 }
                }
            }
        },
        {
            LocationArchetypes.Tavern, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Resonance, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Pressure, TargetValue = ValueTypes.Pressure, TransformationType = TransformationType.Reduce }
                        }
                    },
                    {
                        ValueTypes.Outcome, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Outcome, TargetValue = ValueTypes.Resonance, TransformationType = TransformationType.Increase }
                        }
                    }
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Social, 1 }
                }
            }
        },
        {
            LocationArchetypes.Forest, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Pressure, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Outcome, TargetValue = ValueTypes.Outcome, TransformationType = TransformationType.Reduce }
                        }
                    },
                    {
                        ValueTypes.Insight, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Pressure, TargetValue = ValueTypes.Pressure, TransformationType = TransformationType.Reduce }
                        }
                    }
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Physical, 1 }
                }
            }
        },
        {
            LocationArchetypes.Market, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Outcome, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Outcome, TargetValue = ValueTypes.Resonance, TransformationType = TransformationType.Set }
                        }
                    },
                    {
                        ValueTypes.Resonance, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Energy, TargetValue = ValueTypes.Energy, TransformationType = TransformationType.Reduce }
                        }
                    }
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Social, 1 }
                }
            }
        },
        {
            LocationArchetypes.Workshop, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Insight, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Energy, TargetValue = ValueTypes.Energy, TransformationType = TransformationType.Reduce }
                        }
                    },
                    {
                        ValueTypes.Outcome, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Pressure, TargetValue = ValueTypes.Pressure, TransformationType = TransformationType.Increase }
                        }
                    },
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Focus, 1 }
                }
            }
        },
        {
            LocationArchetypes.Docks, new ArchetypeEffect
            {
                ValueTransformations = new Dictionary<ValueTypes, List<ValueTransformation>>
                {
                    {
                        ValueTypes.Outcome, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Pressure, TargetValue = ValueTypes.Pressure, TransformationType = TransformationType.Increase }
                        }
                    },
                    {
                        ValueTypes.Insight, new List<ValueTransformation>
                        {
                            new ValueTransformation { SourceValue = ValueTypes.Energy, TargetValue = ValueTypes.Energy, TransformationType = TransformationType.Reduce }
                        }
                    }
                },
                EnergyCostReductions = new Dictionary<EnergyTypes, int>
                {
                    { EnergyTypes.Physical, 1 }
                }
            }
        }
    };
}
